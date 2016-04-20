namespace CloudCopy.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Azure.Core;
    using CloudCopy.Core;
    using CloudCopy.Helpers;

    public class CopyCommand : ICommand
    {
        ICopier copier;
        IBlobsListing listing;

        public CopyCommand()
            : this(new CloudCopy.Azure.Copier(), new CloudCopy.Azure.BlobsListing())
        {
        }

        public CopyCommand(ICopier copier, IBlobsListing listing)
        {
            this.copier = copier;
            this.listing = listing;
        }

        public bool Execute()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Copying blobs (press ESC to cancel)...");
            Console.ResetColor();
            Console.WriteLine();

            var blobs = (new ListBlobsHelper(this.listing)).ListBlobsFromArgs();

            if (blobs.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("No blobs found!");
                Console.ResetColor();

                return false;
            }

            var cts = new CancellationTokenSource();
            var completed = false;

            if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
            {
                ArgSettings.SourceConnection = "UseDevelopmentStorage=true";
            }

            if (string.IsNullOrEmpty(ArgSettings.DestinationConnection))
            {
                ArgSettings.DestinationConnection = ArgSettings.SourceConnection;
            }

            var task = Task.Factory.StartNew(() =>
            {
                copier.CopyBlobs(string.Empty,
                    blobs,
                    ArgSettings.SourceConnection,
                    ArgSettings.Destination,
                    ArgSettings.DestinationConnection,
                    cts.Token,
                    (f) =>
                    {
                        Console.WriteLine(f);
                    },
                    (f, e) =>
                    {
                        Console.Error.WriteLine("Failed to copy {0}: {1}", f, e.ToString());
                    },
                    () =>
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.WriteLine("Finished copying blobs.");
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