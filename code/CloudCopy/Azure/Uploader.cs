namespace CloudCopy.Azure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Core;
    using CloudCopy.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class Uploader : IUploader
    {
        private CloudBlobContainer container;
        private CloudBlobDirectory directory;

        public void UploadFiles(string basePath, IList<string> files, string storageConnectionString, string destination, CancellationToken cancelToken, Action<string> fileUploadedAction, Action<string, Exception> fileFailedAction, Action completedAction)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            try
            {
                container = blobClient.GetContainerReference(destination);
                container.CreateIfNotExist();

                if (ArgSettings.SetPublic)
                {
                    container.SetPermissions(new BlobContainerPermissions() { PublicAccess = BlobContainerPublicAccessType.Container });
                }
            }
            catch (StorageClientException)
            {
                container = null;
                directory = blobClient.GetBlobDirectoryReference(destination);
            }

            UploadFiles(basePath, files, cancelToken, fileUploadedAction, fileFailedAction, completedAction);
        }

        private void UploadFiles(string basePath, IList<string> files, CancellationToken cancelToken, Action<string> fileUploadedAction, Action<string, Exception> fileFailedAction, Action completedAction)
        {
            try
            {
                Parallel.ForEach(
                    files,
                    new ParallelOptions { MaxDegreeOfParallelism = ArgSettings.Parallelism, CancellationToken = cancelToken },
                    (filePath, loopState) =>
                    {
                        var blobName = Path.GetFileName(filePath);

                        if (!string.IsNullOrEmpty(basePath))
                        {
                            if (basePath.EndsWith("\\") || basePath.EndsWith("/"))
                            {
                                blobName = filePath.Substring(basePath.Length);
                            }
                            else
                            {
                                blobName = filePath.Substring(basePath.Length + 1);
                            }
                        }

                        var blob = this.GetBlobReference(blobName);
                        var fileUploaded = false;

                        Stream fileStream = null;

                        while (!fileUploaded && !loopState.IsStopped)
                        {
                            try
                            {
                                if (ArgSettings.UseGZip)
                                {
                                    blob.Properties.ContentEncoding = "gzip";
                                    blob.Properties.ContentType = MimeTypeHelper.MimeType(filePath);

                                    fileStream = GZipHelper.Zip(filePath);
                                }
                                else
                                {
                                    fileStream = File.Open(filePath, FileMode.Open);
                                }

                                var hash = HashHelper.ComputeMD5(fileStream);

                                // check blob is already uploaded and is the same content
                                if (blob.Exists())
                                {
                                    if (blob.Attributes.Properties.ContentMD5 == hash && blob.Attributes.Properties.Length == fileStream.Length)
                                    {
                                        fileUploaded = true;
                                    }
                                    else
                                    {
                                        //delete obsolete existing blob
                                        blob.Delete();
                                    }
                                }

                                if (!fileUploaded)
                                {
                                    blob.Properties.ContentMD5 = hash;

                                    blob.UploadFromStream(fileStream,
                                        new BlobRequestOptions() {
                                            RetryPolicy = RetryPolicies.Retry(ArgSettings.RetryCount, TimeSpan.FromMilliseconds(ArgSettings.RetryInterval))
                                        });

                                    fileUploaded = true;
                                }

                                if (!loopState.IsStopped && fileUploaded && fileUploadedAction != null)
                                {
                                    fileUploadedAction(blobName);
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
                            finally
                            {
                                if (!loopState.IsStopped && fileStream != null)
                                {
                                    fileStream.Close();
                                }
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

        private CloudBlob GetBlobReference(string blobName)
        {
            if (container != null)
            {
                return container.GetBlobReference(blobName);
            }
            else
            {
                return directory.GetBlobReference(blobName);
            }
        }
    }
}
