namespace CloudCopy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface ICopier
    {
        void CopyBlobs(string baseDirectoryUri,
                             IList<string> sourceBlobs,
                             string sourceConnectionString,
                             string destinationUri,
                             string destConnectionString,
                             CancellationToken cancelToken,
                             Action<string> blobCopiedAction,
                             Action<string, Exception> blobFailedAction,
                             Action completedAction);
    }
}
