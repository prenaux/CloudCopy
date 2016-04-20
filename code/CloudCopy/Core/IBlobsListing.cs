namespace CloudCopy.Azure.Core
{
    using System.Collections.Generic;

    public interface IBlobsListing
    {
        IList<string> ListBlobs(string directoryUri, string pattern, string storageConnectionString, bool flatten);
    }
}
