namespace CloudCopy.Azure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Core;
    using CloudCopy.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class Downloader : IDownloader
    {
        public void DownloadFiles(string baseDirectoryUri,
                          IList<string> blobs,
                          string destination,
                          string storageConnectionString,
                          CancellationToken cancelToken,
                          Action<string> fileDownloadedAction,
                          Action<string, Exception> fileFailedAction,
                          Action completedAction)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var baseBlobLength = 0;

            if (!string.IsNullOrEmpty(baseDirectoryUri))
            {
                baseBlobLength = baseDirectoryUri.Length + (baseDirectoryUri.EndsWith("/") ? 0 : 1);
            }
            else
            {
                var shortestBlob = blobs.OrderBy(s => s.Length).First();
                baseBlobLength = shortestBlob.LastIndexOf("/") + 1;
            }

            try
            {
                Parallel.ForEach(
                    blobs,
                    new ParallelOptions { MaxDegreeOfParallelism = ArgSettings.Parallelism, CancellationToken = cancelToken },
                    (blobName, loopState) =>
                    {
                        var blob = blobClient.GetBlobReference(blobName);
                        var blockBlob = blob.ToBlockBlob;
                        var destPath = Path.Combine(destination, blobName.Substring(baseBlobLength));
                        var fileDownloaded = false;

                        while (!fileDownloaded && !loopState.IsStopped)
                        {
                            try
                            {
                                blob.FetchAttributes();
                                var isGZip = blob.Properties.ContentEncoding != null && blob.Properties.ContentEncoding.Equals("gzip", StringComparison.OrdinalIgnoreCase);

                                if (File.Exists(destPath))
                                {
                                    var hash = string.Empty;

                                    if (isGZip)
                                    {
                                        using (var fileStream = GZipHelper.Zip(destPath))
                                        {
                                            hash = HashHelper.ComputeMD5(fileStream);
                                        }
                                    }
                                    else
                                    {
                                        using (var fileStream = File.Open(destPath, FileMode.Open))
                                        {
                                            hash = HashHelper.ComputeMD5(fileStream);
                                        }
                                    }

                                    if (blob.Attributes.Properties.ContentMD5 == hash)
                                    {
                                        fileDownloaded = true;
                                    }
                                    else
                                    {
                                        //delete obsolete existing file
                                        File.Delete(destPath);
                                    }
                                }

                                if (!fileDownloaded)
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                                    if (isGZip)
                                    {
                                        using (var stream = new MemoryStream())
                                        {
                                            blob.DownloadToStream(stream, new BlobRequestOptions()
                                            {
                                                RetryPolicy = RetryPolicies.Retry(ArgSettings.RetryCount, TimeSpan.FromMilliseconds(ArgSettings.RetryInterval))
                                            });

                                            stream.Seek(0, SeekOrigin.Begin);

                                            GZipHelper.Unzip(stream, destPath);
                                        }
                                    }
                                    else
                                    {
                                        blob.DownloadToFile(destPath, new BlobRequestOptions()
                                        {
                                            RetryPolicy = RetryPolicies.Retry(ArgSettings.RetryCount, TimeSpan.FromMilliseconds(ArgSettings.RetryInterval))
                                        });
                                    }


                                    fileDownloaded = true;
                                }

                                if (!loopState.IsStopped && fileDownloaded && fileDownloadedAction != null)
                                {
                                    fileDownloadedAction(blobName);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (!loopState.IsStopped && fileFailedAction != null)
                                {
                                    fileFailedAction(blobName, ex);
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
