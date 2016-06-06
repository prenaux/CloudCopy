namespace CloudCopy.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Azure.Core;
    using CloudCopy.Core;
    using CloudCopy.Helpers;

    public class RemoveCommand : ICommand
    {
        IRemover remover;
        IBlobsListing listing;

        public RemoveCommand()
            : this(new CloudCopy.Azure.Remover(), new CloudCopy.Azure.BlobsListing())
        {
        }

        public RemoveCommand(IRemover remover, IBlobsListing listing)
        {
            this.remover = remover;
            this.listing = listing;
        }

        public bool Execute()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Removing blobs (press ESC to cancel)...");
            Console.ResetColor();
            Console.WriteLine();

            var cts = new CancellationTokenSource();
            var completed = false;

            if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
            {
                ArgSettings.SourceConnection = "UseDevelopmentStorage=true";
            }

            var task = Task.Factory.StartNew(() =>
            {
                remover.RemoveFiles(ArgSettings.Source,
                    ArgSettings.SourceConnection,
                    cts.Token,
                    (f) =>
                    {
                        Console.WriteLine("Removed {0}", f);
                    },
                    (f, e) =>
                    {
                        Console.Error.WriteLine("Failed to remove {0}: {1}", f, e.ToString());
                    },
                    () =>
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.WriteLine("Finished removing blobs.");
                        Console.ResetColor();
                        completed = true;
                    });
            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            while (!completed)
            {
                if (ArgumentsHelper.IsKeyAvailable())
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