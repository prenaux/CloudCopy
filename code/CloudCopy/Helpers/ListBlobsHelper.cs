namespace CloudCopy.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CloudCopy.Azure.Core;

    public class ListBlobsHelper
    {
        IBlobsListing listing;

        public ListBlobsHelper() 
            : this (new CloudCopy.Azure.BlobsListing())
        {
        }

        public ListBlobsHelper(IBlobsListing listing)
        {
            this.listing = listing;
        }

        public IList<string> ListBlobsFromArgs()
        {
            var directoryUri = ArgSettings.Source;
            var searchPattern = string.Empty;

            if (!string.IsNullOrEmpty(directoryUri))
            {
                var lastSegment = new Uri(ArgSettings.Source).Segments.Last();

                if (lastSegment.Contains('*') ||
                    lastSegment.Contains('?'))
                {
                    directoryUri = ArgSettings.Source.Remove(ArgSettings.Source.Length - lastSegment.Length);
                    searchPattern = lastSegment;
                }
            }

            var blobs = this.listing.ListBlobs(directoryUri, searchPattern, ArgSettings.SourceConnection, true);

            return blobs;
        }
    }
}
