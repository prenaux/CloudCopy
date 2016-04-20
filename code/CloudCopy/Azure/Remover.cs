namespace CloudCopy.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Azure.Core;
    using CloudCopy.Core;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class Remover : IRemover
    {
        private CloudStorageAccount storageAccount;
        private CloudBlobClient blobClient;
        private IBlobsListing listing;

        public Remover()
            : this(new BlobsListing())
        {
        }

        public Remover(IBlobsListing listing)
        {
            this.listing = listing;
        }

        public void RemoveFiles(string directoryUri, string storageConnectionString, CancellationToken cancelToken, Action<string> fileDeletedAction, Action<string, Exception> fileFailedAction, Action completedAction)
        {
            storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();

            var directoryPath = directoryUri;
            var searchPattern = string.Empty;
            var lastSegment = directoryUri.Substring(directoryUri.LastIndexOf("/") + 1);

            if (lastSegment.Contains('*') ||
                lastSegment.Contains('?'))
            {
                directoryPath = directoryUri.Remove(directoryUri.Length - lastSegment.Length);
                searchPattern = lastSegment;

                var blobs = listing.ListBlobs(directoryPath, searchPattern, storageConnectionString, true);
                this.DeleteBlobs(blobs, cancelToken, fileDeletedAction, fileFailedAction);
            }
            else
            {
                try
                {
                    var container = blobClient.GetContainerReference(directoryUri);
                    container.Delete();

                    if (fileDeletedAction != null)
                    {
                        fileDeletedAction(directoryUri);
                    }
                }
                catch (StorageClientException)
                {
                    try
                    {
                        var blob = blobClient.GetBlobReference(directoryUri);
                        blob.Delete();

                        if (fileDeletedAction != null)
                        {
                            fileDeletedAction(directoryUri);
                        }
                    }
                    catch (StorageClientException)
                    {
                        try
                        {
                            var directory = blobClient.GetBlobDirectoryReference(directoryUri);
                            var blobs = directory.ListBlobs().Select(i => i.Uri.ToString()).ToArray();

                            if (blobs.Length > 0)
                            {
                                this.DeleteBlobs(blobs, cancelToken, fileDeletedAction, fileFailedAction);
                            }
                        }
                        catch (StorageClientException ex)
                        {
                            if (fileFailedAction != null)
                            {
                                fileFailedAction(directoryUri, ex);
                            }
                        }
                    }
                }
            }

            if (completedAction != null)
            {
                completedAction();
            }
        }

        private void DeleteBlobs(IList<string> blobs, CancellationToken cancelToken, Action<string> fileDeletedAction, Action<string, Exception> fileFailedAction)
        {
            Parallel.ForEach(
                blobs,
                new ParallelOptions { MaxDegreeOfParallelism = ArgSettings.Parallelism, CancellationToken = cancelToken },
                (blob, loopState) =>
                {
                    try
                    {
                        blobClient.GetBlobReference(blob).Delete();

                        if (!loopState.IsStopped && fileDeletedAction != null)
                        {
                            fileDeletedAction(blob);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!loopState.IsStopped && fileFailedAction != null)
                        {
                            fileFailedAction(blob, ex);
                        }
                    }
                });
        }
    }
}
