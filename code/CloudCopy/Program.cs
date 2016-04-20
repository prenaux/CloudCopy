namespace CloudCopy
{
    using System;
    using CloudCopy.Commands;
    using CloudCopy.Helpers;

    class Program
    {
        public static int Main(string[] args)
        {
            // cloudcopy.exe "source" "destination" "blobcnstring (default dev storage)" -flags
            // cloudcopy.exe "C:\Test\*.*" "mycontainer/myfolder" "DefaultEndpointsProtocal=http;AccountName=userdevelopment;AccountKey=accKey"
            // cloudcopy.exe "C:\Test\*.*" "/mycontainer/myfolder" "DefaultEndpointsProtocal=http;AccountName=userdevelopment;AccountKey=accKey"
            // cloudcopy.exe "/mycontainer/myfolder" "C:\Test\*.*" "DefaultEndpointsProtocal=http;AccountName=userdevelopment;AccountKey=accKey"
            // cloudcopy.exe "/mycontainer/myfolder" "/mycontainer/myfolder" "DefaultEndpointsProtocal=http;AccountName=userdevelopmentA;AccountKey=accKeyA" "DefaultEndpointsProtocal=http;AccountName=userdevelopmentB;AccountKey=accKeyB"
            // cloudcopy.exe "C:\Test\*.*" "/mycontainer/myfolder"
            // cloudcopy.exe "/mycontainer/myfolder" "DefaultEndpointsProtocal=http;AccountName=userdevelopment;AccountKey=accKey" -L

            if (ArgumentsHelper.Parse(args))
            {
                return RunCommands() ? 0 : 1;
            }
            else
            {
                return 1; 
            }
        }

        private static bool RunCommands()
        {
            ICommand cmd = new HelpCommand();

            if (!string.IsNullOrEmpty(ArgSettings.Help))
            {
                cmd = new HelpCommand();
            }
            else if (ArgSettings.List)
            {
                cmd = new ListCommand();
            }
            else if (ArgSettings.Remove)
            {
                cmd = new RemoveCommand();
            }
            else if (ArgumentsHelper.IsValidUri(ArgSettings.Source) &&
                ArgumentsHelper.IsValidUri(ArgSettings.Destination) &&
                ArgumentsHelper.IsValidAzureConnection(ArgSettings.SourceConnection))
            {
                cmd = new CopyCommand();
            }
            else if (ArgumentsHelper.IsValidUri(ArgSettings.Source) &&
                ArgumentsHelper.IsValidFileSystemPath(ArgSettings.Destination))
            {
                cmd = new DownloadCommand();
            }
            else if (ArgumentsHelper.IsValidFileSystemPath(ArgSettings.Source) &&
                ArgumentsHelper.IsValidUri(ArgSettings.Destination))
            {
                cmd = new UploadCommand();
            }

            return cmd.Execute();
        }
    }
}
