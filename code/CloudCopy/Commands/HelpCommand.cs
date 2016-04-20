namespace CloudCopy.Commands
{
    using System;

    public class HelpCommand : ICommand
    {
        public bool Execute()
        {
            ShowWelcome();

            if (string.IsNullOrEmpty(ArgSettings.Help) || ArgSettings.Help == "FULL")
            {
                this.ShowFullHelp();

                return true;
            }
            else
            {
                return this.ShowCommandHelp(ArgSettings.Help);
            }
        }

        private static void ShowWelcome()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("CloudCopy Tool 1.0");
            Console.ResetColor();
            Console.WriteLine(" by Jonathan Cisneros");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("http://cloudcopy.codeplex.com");
            Console.ResetColor();
            Console.WriteLine();
        }

        private void ShowFullHelp()
        {
            Console.WriteLine("Download, upload, remove and copy Azure blobs.");
            Console.WriteLine();
            Console.WriteLine("Syntax:");
            Console.WriteLine("=======");
            Console.WriteLine("CloudCopy.exe \"source\" \"dest\" [\"source connection\"] [\"dest connection\"] [-Flags]");
            Console.WriteLine();

            Console.WriteLine(" - source / dest:");
            Console.WriteLine("   filesystem location or Blob storage URI (can include wildchards * ?)");
            Console.WriteLine(" - source connection / dest connection:");
            Console.WriteLine("   Blob storage connection string");
            Console.WriteLine(" - Flags:");

            var properties = typeof(ArgSettings).GetProperties();

            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes(typeof(FlagAttribute), false);
                if (attrs.Length == 1)
                {
                    var flagAttr = (FlagAttribute)attrs[0];
                    var line = string.Format("   -{0} {1} ", flagAttr.Acronim + new string(' ', 4 - flagAttr.Acronim.Length), flagAttr.Description);

                    Console.WriteLine(line);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Samples");
            Console.WriteLine("=======");
            Console.WriteLine();
            Console.WriteLine("- List txt blobs in container:");
            Console.WriteLine("  CloudCopy.exe \"https://myaccount.blob.core.windows.net/mycontainer/*.txt\"  \"DefaultEndpointsProtocol=http;AccountName=user;AccountKey=key\" -L");
            Console.WriteLine();
            Console.WriteLine("- Remove txt blobs in container:");
            Console.WriteLine("  CloudCopy.exe \"https://myaccount.blob.core.windows.net/mycontainer/*.txt\"  \"DefaultEndpointsProtocol=http;AccountName=user;AccountKey=key\" -R");
            Console.WriteLine();
            Console.WriteLine("- Download container to filesystem:");
            Console.WriteLine("  CloudCopy.exe \"https://myaccount.blob.core.windows.net/mycontainer\" \"C:\\Temp\" \"DefaultEndpointsProtocol=http;AccountName=user;AccountKey=key\"");
            Console.WriteLine();
            Console.WriteLine("- Download jpg files from a directory:");
            Console.WriteLine("  CloudCopy.exe \"https://myaccount.blob.core.windows.net/mycontainer/mydir/*.jpg\" \"C:\\Temp\" \"DefaultEndpointsProtocol=http;AccountName=user;AccountKey=key\"");
            Console.WriteLine();
            Console.WriteLine("- Upload text files to a container:");
            Console.WriteLine("  CloudCopy.exe \"C:\\Temp\\*.txt\" \"https://myaccount.blob.core.windows.net/mycontainer/\"  \"DefaultEndpointsProtocol=http;AccountName=user;AccountKey=key\"");
            Console.WriteLine();
            Console.WriteLine("- Copy jpg blobs to another storages:");
            Console.WriteLine("  CloudCopy.exe \"https://sourceaccount.blob.core.windows.net/sourcecontainer/*.jpg\" \"https://destaccount.blob.core.windows.net/destcontainer/\"  \"DefaultEndpointsProtocol=http;AccountName=sourceuser;AccountKey=sourcekey\" \"DefaultEndpointsProtocal=http;AccountName=destuser;AccountKey=destkey\"");
        }

        private bool ShowCommandHelp(string command)
        {
            var properties = typeof(ArgSettings).GetProperties();
            var found = false;

            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes(typeof(FlagAttribute), false);
                if (attrs.Length == 1)
                {
                    var flagAttr = (FlagAttribute)attrs[0];

                    if (command.Equals(flagAttr.Acronim, StringComparison.OrdinalIgnoreCase) ||
                        command.Equals("-" + flagAttr.Acronim, StringComparison.OrdinalIgnoreCase) ||
                        command.Equals("/" + flagAttr.Acronim, StringComparison.OrdinalIgnoreCase) ||
                        command.Equals(property.Name, StringComparison.OrdinalIgnoreCase) ||
                        command.Equals("-" + property.Name, StringComparison.OrdinalIgnoreCase) ||
                        command.Equals("/" + property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Description:");
                        Console.WriteLine("  {0}", flagAttr.Description);
                        Console.WriteLine();
                        Console.WriteLine("Flag name:");
                        Console.WriteLine("  -{0}", property.Name);
                        Console.WriteLine();
                        Console.WriteLine("Abbreviated flag:");
                        Console.WriteLine("  -{0}", flagAttr.Acronim);
                        Console.WriteLine();
                        if (!string.IsNullOrEmpty(flagAttr.Sample))
                        {
                            Console.WriteLine("Sample:");
                            Console.WriteLine("  {0}", flagAttr.Sample);
                        }

                        found = true;
                    }
                }
            }

            if (!found)
            {
                Console.WriteLine();
                Console.Error.WriteLine("Unrecognized parameter {0}", command);

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
