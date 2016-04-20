namespace CloudCopy.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static class PathsHelper
    {
        public static string GetDirectory(string path)
        {
            var directoryPath = path;
            var searchPattern = string.Empty;
            var lastSegment = path.Substring(path.LastIndexOf("\\") + 1);

            if (lastSegment.Contains('*') ||
                lastSegment.Contains('?'))
            {
                directoryPath = path.Remove(path.Length - lastSegment.Length);
            }
            else
            {
                if (File.Exists(path))
                {
                    directoryPath = Path.GetDirectoryName(path);
                }
            }

            return directoryPath;
        }

        public static List<string> ListFiles(string path)
        {
            var directoryPath = path;
            var searchPattern = string.Empty;
            var lastSegment = path.Substring(path.LastIndexOf("\\") + 1);

            if (lastSegment.Contains('*') ||
                lastSegment.Contains('?'))
            {
                directoryPath = path.Remove(path.Length - lastSegment.Length);
                searchPattern = lastSegment;
            }
            else
            {
                searchPattern = "*";

                // it's a single file
                if (File.Exists(path))
                {
                    return new List<string> { path };
                }
            }

            return GetFilesRecursive(directoryPath, searchPattern);
        }

        public static List<string> GetFilesRecursive(string directoryPath, string searchPattern)
        {
            var result = new List<string>();
            var stack = new Stack<string>();

            stack.Push(directoryPath);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                try
                {
                    result.AddRange(Directory.GetFiles(dir, searchPattern));

                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        public static string[] FindMatches(string pattern, string[] names)
        {
            List<string> matches = new List<string>();
            Regex regex = FindFilesPatternToRegex.Convert(pattern);

            foreach (string s in names)
            {
                if (regex.IsMatch(s))
                {
                    matches.Add(s);
                }
            }

            return matches.ToArray();
        }

        public static bool MatchPattern(string pattern, string path)
        {
            List<string> matches = new List<string>();
            Regex regex = FindFilesPatternToRegex.Convert(pattern);

            return regex.IsMatch(path);
        }

        internal static class FindFilesPatternToRegex
        {
            private static Regex HasQuestionMarkRegEx = new Regex(@"\?", RegexOptions.Compiled);
            private static Regex HasAsteriskRegex = new Regex(@"\*", RegexOptions.Compiled);
            private static Regex IlegalCharactersRegex = new Regex("[:<>|\"]", RegexOptions.Compiled);
            private static Regex CatchExtentionRegex = new Regex(@"^\s*.+\.([^\.]+)\s*$", RegexOptions.Compiled);
            private static string NonDotCharacters = @"[^.]*";

            public static Regex Convert(string pattern)
            {
                if (pattern == null)
                {
                    throw new ArgumentNullException();
                }

                pattern = pattern.Trim();

                if (pattern.Length == 0)
                {
                    throw new ArgumentException("Pattern is empty.");
                }

                if (IlegalCharactersRegex.IsMatch(pattern))
                {
                    throw new ArgumentException("Patterns contains ilegal characters.");
                }

                bool hasExtension = CatchExtentionRegex.IsMatch(pattern);
                bool matchExact = false;

                if (HasQuestionMarkRegEx.IsMatch(pattern))
                {
                    matchExact = true;
                }
                else if (hasExtension)
                {
                    matchExact = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
                }

                string regexString = Regex.Escape(pattern);
                regexString = "^" + Regex.Replace(regexString, @"\\\*", ".*");
                regexString = Regex.Replace(regexString, @"\\\?", ".");

                if (!matchExact && hasExtension)
                {
                    regexString += NonDotCharacters;
                }

                regexString += "$";
                Regex regex = new Regex(regexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);

                return regex;
            }
        }
    }
}
