namespace CloudCopy.Azure
{
    using System.Collections.Generic;
    using System.Linq;
    using CloudCopy.Azure.Core;
    using CloudCopy.Helpers;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class BlobsListing : IBlobsListing
    {
        public IList<string> ListBlobs(string directoryUri, string pattern, string storageConnectionString, bool flatten)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            List<IListBlobItem> blobs;

            if (string.IsNullOrEmpty(directoryUri))
            {
                blobs = blobClient.ListBlobsWithPrefix(string.Empty, new BlobRequestOptions() { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = flatten }).ToList();
            }
            else
            {
                var container = blobClient.GetContainerReference(directoryUri);

                try
                {
                    container.FetchAttributes();
                    blobs = container.ListBlobs(new BlobRequestOptions() { UseFlatBlobListing = flatten }).ToList();
                }
                catch (StorageClientException)
                {
                    try
                    {
                        var directory = blobClient.GetBlobDirectoryReference(directoryUri);
                        blobs = directory.ListBlobs(new BlobRequestOptions() { UseFlatBlobListing = flatten }).ToList();
                    }
                    catch
                    {
                        blobs = blobClient.ListBlobsWithPrefix(string.Empty, new BlobRequestOptions() { BlobListingDetails = BlobListingDetails.Metadata, UseFlatBlobListing = flatten }).ToList();
                    }
                }
            }

            var result = new List<string>();

            foreach (var blob in blobs)
            {
                if (string.IsNullOrEmpty(pattern) || PathsHelper.MatchPattern(pattern, blob.Uri.ToString()))
                {
                    result.Add(blob.Uri.ToString());
                }
            }

            return result;
        }
    }
}
