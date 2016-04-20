namespace CloudCopy.Helpers
{
    using System;
    using System.IO;
    using Microsoft.WindowsAzure;

    public class ArgumentsHelper
    {
        public static bool Parse(string[] args)
        {
            var properties = typeof(ArgSettings).GetProperties();
            var nextValue = string.Empty;
            var currentValue = string.Empty;
            var assigned = false;

            for (int i = 0; i < args.Length; i++)
            {
                nextValue = (i == args.Length - 1) ? string.Empty : args[i + 1];
                currentValue = args[i];
                assigned = false;

                foreach (var property in properties)
                {
                    var attrs = property.GetCustomAttributes(typeof(FlagAttribute), false);
                    if (attrs.Length == 1)
                    {
                        var flagAttr = (FlagAttribute)attrs[0];

                        if (args[i].Equals(flagAttr.Acronim, StringComparison.OrdinalIgnoreCase) ||
                            args[i].Equals("-" + flagAttr.Acronim, StringComparison.OrdinalIgnoreCase) ||
                            args[i].Equals("/" + flagAttr.Acronim, StringComparison.OrdinalIgnoreCase))
                        {
                            if (flagAttr.Acronim == "?" && string.IsNullOrEmpty(nextValue))
                            {
                                nextValue = "FULL";
                            }

                            if (property.PropertyType == typeof(bool))
                            {
                                property.SetValue(null, true, null);
                            }
                            else
                            {
                                property.SetValue(null, nextValue, null);
                                i++;
                            }

                            assigned = true;
                            break;
                        }
                    }
                }

                if (!assigned)
                {
                    var validValue = true;

                    if (IsValidUri(currentValue) || IsValidFileSystemPath(currentValue))
                    {
                        if (string.IsNullOrEmpty(ArgSettings.Source))
                        {
                            ArgSettings.Source = currentValue;
                        }
                        else if (string.IsNullOrEmpty(ArgSettings.Destination))
                        {
                            ArgSettings.Destination = currentValue;
                        }
                        else
                        {
                            validValue = false;
                        }
                    }
                    else if (IsValidAzureConnection(currentValue))
                    {
                        if (!string.IsNullOrEmpty(ArgSettings.Source) && !string.IsNullOrEmpty(ArgSettings.Destination))
                        {
                            if ((IsValidUri(ArgSettings.Source) && IsValidFileSystemPath(ArgSettings.Destination)) ||
                                (IsValidUri(ArgSettings.Destination) && IsValidUri(ArgSettings.Source)))
                            {
                                // it is download
                                if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
                                {
                                    ArgSettings.SourceConnection = currentValue;
                                }
                                else if (string.IsNullOrEmpty(ArgSettings.DestinationConnection))
                                {
                                    ArgSettings.DestinationConnection = currentValue;
                                }
                            }
                            else if (IsValidUri(ArgSettings.Destination) && IsValidFileSystemPath(ArgSettings.Source))
                            {
                                // it is upload
                                if (string.IsNullOrEmpty(ArgSettings.DestinationConnection))
                                {
                                    ArgSettings.DestinationConnection = currentValue;
                                }
                                else if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
                                {
                                    ArgSettings.SourceConnection = currentValue;
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(ArgSettings.SourceConnection))
                            {
                                ArgSettings.SourceConnection = currentValue;
                            }
                            else if (string.IsNullOrEmpty(ArgSettings.DestinationConnection))
                            {
                                ArgSettings.DestinationConnection = currentValue;
                            }
                            else
                            {
                                // it is a third cn string
                                validValue = false;
                            }
                        }
                    }
                    else
                    {
                        validValue = false;
                    }

                    if (!validValue)
                    {
                        Console.WriteLine();
                        Console.Error.WriteLine("Invalid or unrecognized parameter {0}", currentValue);

                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsValidAzureConnection(string value)
        {
            CloudStorageAccount tmp;
            return CloudStorageAccount.TryParse(value, out tmp);
        }

        public static bool IsValidFileSystemPath(string value)
        {
            try
            {
                return !string.IsNullOrEmpty(Path.GetPathRoot(value));
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUri(string value)
        {
            if (IsValidFileSystemPath(value))
            {
                return false;
            }

            Uri tmp;
            return Uri.TryCreate(value, UriKind.Absolute, out tmp);
        }
    }
}
