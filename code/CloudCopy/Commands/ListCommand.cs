namespace CloudCopy.Commands
{
    using System;
    using CloudCopy.Azure.Core;
    using CloudCopy.Helpers;

    public class ListCommand : ICommand
    {
        IBlobsListing listing;

        public ListCommand() 
            : this (new CloudCopy.Azure.BlobsListing())
        {
        }

        public ListCommand(IBlobsListing listing)
        {
            this.listing = listing;
        }

        public bool Execute()
        {
            if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
            {
                ArgSettings.SourceConnection = "UseDevelopmentStorage=true";
            }

            var blobs = (new ListBlobsHelper(this.listing)).ListBlobsFromArgs();

            if (blobs.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No blobs found!");
                Console.ResetColor();

                return false;
            }
            else
            {
                foreach (var blob in blobs)
                {
                    Console.WriteLine(blob);
                }
            }

            return true;
        }
    }
}
