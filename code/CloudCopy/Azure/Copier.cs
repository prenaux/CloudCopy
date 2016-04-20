namespace CloudCopy.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Core;
    using CloudCopy.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class Copier : ICopier
    {
        public void CopyBlobs(string baseDirectoryUri,
                          IList<string> sourceBlobs,
                          string sourceConnectionString,
                          string destinationUri,
                          string destConnectionString,
                          CancellationToken cancelToken,
                          Action<string> blobCopiedAction,
                          Action<string, Exception> blobFailedAction,
                          Action completedAction)
        {
            var storageAccount = CloudStorageAccount.Parse(sourceConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobClient destBlobClient = blobClient;

            if (!destConnectionString.Equals(sourceConnectionString, StringComparison.OrdinalIgnoreCase))
            {
                var destStorageAccount = CloudStorageAccount.Parse(destConnectionString);
                destBlobClient = storageAccount.CreateCloudBlobClient();
            }

            var baseBlobLength = 0;

            if (!string.IsNullOrEmpty(baseDirectoryUri))
            {
                baseBlobLength = baseDirectoryUri.Length + (baseDirectoryUri.EndsWith("/") ? 0 : 1);
            }
            else
            {
                var shortestBlob = sourceBlobs.OrderBy(s => s.Length).First();
                baseBlobLength = shortestBlob.LastIndexOf("/") + 1;
            }

            try
            {
                Parallel.ForEach(
                    sourceBlobs,
                    new ParallelOptions { MaxDegreeOfParallelism = ArgSettings.Parallelism, CancellationToken = cancelToken },
                    (sourceBlobName, loopState) =>
                    {
                        var blob = blobClient.GetBlobReference(sourceBlobName);
                        var blockBlob = blob.ToBlockBlob;
                        string destBlobUri;

                        if (!destinationUri.EndsWith("/"))
                        {
                            destBlobUri = destinationUri + "/" + sourceBlobName.Substring(baseBlobLength);
                        }
                        else
                        {
                            destBlobUri = destinationUri + sourceBlobName.Substring(baseBlobLength);
                        }

                        var destBlob = destBlobClient.GetBlobReference(destBlobUri);
                        var fileCopied = false;

                        try
                        {
                            var tmpContainer = blobClient.GetContainerReference(destinationUri);
                            tmpContainer.CreateIfNotExist();
                        }
                        catch
                        {
                        }

                        while (!fileCopied && !loopState.IsStopped)
                        {
                            try
                            {
                                blob.FetchAttributes();
                                
                                if (destBlob.Exists())
                                {
                                    if (blob.Attributes.Properties.ContentMD5 == destBlob.Attributes.Properties.ContentMD5)
                                    {
                                        fileCopied = true;
                                    }
                                }

                                if (!fileCopied)
                                {
                                    destBlob.CopyFromBlob(blob, new BlobRequestOptions()
                                        {
                                            RetryPolicy = RetryPolicies.Retry(ArgSettings.RetryCount, TimeSpan.FromMilliseconds(ArgSettings.RetryInterval))
                                        });

                                    fileCopied = true;
                                }

                                if (!loopState.IsStopped && fileCopied && blobCopiedAction != null)
                                {
                                    blobCopiedAction(destBlobUri);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!loopState.IsStopped && blobFailedAction != null)
                                {
                                    blobFailedAction(destBlobUri, ex);
                                }

                                Thread.Sleep(ArgSettings.RetryInterval);
                            }
                        }
                    });

                if (completedAction != null)
                {
                    completedAction();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
