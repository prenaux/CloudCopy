namespace CloudCopy.Core
{
    using System;
    using System.Threading;

    public interface IRemover
    {
        void RemoveFiles(string directoryUri, string storageConnectionString, CancellationToken cancelToken, Action<string> fileDeletedAction, Action<string, Exception> fileFailedAction, Action completedAction);
    }
}
