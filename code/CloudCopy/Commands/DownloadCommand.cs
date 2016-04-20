namespace CloudCopy.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Azure.Core;
    using CloudCopy.Core;
    using CloudCopy.Helpers;

    public class DownloadCommand : ICommand
    {
        IDownloader downloader;
        IBlobsListing listing;

        public DownloadCommand()
            : this(new CloudCopy.Azure.Downloader(), new CloudCopy.Azure.BlobsListing())
        {
        }

        public DownloadCommand(IDownloader downloader, IBlobsListing listing)
        {
            this.downloader = downloader;
            this.listing = listing;
        }

        public bool Execute()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Downloading blobs (press ESC to cancel)...");
            Console.ResetColor();
            Console.WriteLine();

            var blobs = (new ListBlobsHelper(this.listing)).ListBlobsFromArgs();
            var cts = new CancellationTokenSource();
            var completed = false;

            if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
            {
                ArgSettings.SourceConnection = "UseDevelopmentStorage=true";
            }

            var task = Task.Factory.StartNew(() =>
            {
                downloader.DownloadFiles(string.Empty,
                    blobs,
                    ArgSettings.Destination,
                    ArgSettings.SourceConnection,
                    cts.Token,
                    (f) =>
                    {
                        Console.WriteLine(f);
                    },
                    (f, e) =>
                    {
                        Console.Error.WriteLine("Failed to download {0}: {1}", f, e.ToString());
                    },
                    () =>
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.WriteLine("Finished downloading files.");
                        Console.ResetColor();
                        completed = true;
                    });
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            while (!completed)
            {
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Cancelled!");
                        Console.ResetColor();

                        return false;
                    }
                }
            }

            return true;
        }
    }
}
