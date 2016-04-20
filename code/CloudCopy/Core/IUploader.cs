namespace CloudCopy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IUploader
    {
        void UploadFiles(string basePath, 
                          IList<string> files, 
                          string storageConnectionString, 
                          string container, 
                          CancellationToken cancelToken, 
                          Action<string> fileUploadedAction, 
                          Action<string, Exception> fileFailedAction, 
                          Action completedAction);
    }
}
