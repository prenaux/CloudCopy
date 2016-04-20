namespace CloudCopy.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CloudCopy.Core;
    using CloudCopy.Helpers;

    public class UploadCommand : ICommand
    {
        IUploader uploader;

        public UploadCommand()
            : this(new CloudCopy.Azure.Uploader())
        {
        }

        public UploadCommand(IUploader uploader)
        {
            this.uploader = uploader;
        }

        public bool Execute()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Uploading files (press ESC to cancel)...");
            Console.ResetColor();
            Console.WriteLine();

            var cts = new CancellationTokenSource();
            var completed = false;

            if (string.IsNullOrEmpty(ArgSettings.DestinationConnection))
            {
                ArgSettings.DestinationConnection = "UseDevelopmentStorage=true";
            }

            var task = Task.Factory.StartNew(() =>
            {
                uploader.UploadFiles(PathsHelper.GetDirectory(ArgSettings.Source),
                    PathsHelper.ListFiles(ArgSettings.Source),
                    ArgSettings.DestinationConnection,
                    ArgSettings.Destination,
                    cts.Token,
                    (f) =>
                    {
                        Console.WriteLine(f);
                    },
                    (f, e) =>
                    {
                        Console.Error.WriteLine("Failed to upload {0}: {1}", f, e.ToString());
                    },
                    () =>
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine();
                        Console.WriteLine("Finished uploading files.");
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
