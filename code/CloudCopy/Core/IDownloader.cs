namespace CloudCopy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IDownloader
    {
        void DownloadFiles(string basePath,
                          IList<string> blobs,
                          string destination,
                          string storageConnectionString,
                          CancellationToken cancelToken,
                          Action<string> fileDownloadedAction,
                          Action<string, Exception> fileFailedAction,
                          Action completedAction);
    }
}
