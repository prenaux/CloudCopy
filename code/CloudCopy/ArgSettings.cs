namespace CloudCopy
{
    public static class ArgSettings
    {
        private static string help;
        private static string source;
        private static string destination;
        private static string sourceConnection;
        private static string destinationConnection;
        private static bool useGZip;
        private static bool setPublic;
        private static bool list;
        private static bool remove;
        private static int parallelism = 2;
        private static int retryCount = 10;
        private static int retryInterval = 5000; // 5 seconds
        private static int blockSize = 5242880; // 5 MB

        [Flag("?", "Show help")]
        public static string Help
        {
            get { return help; }
            set { help = value; }
        }

        [Flag("S", "Source path")]
        public static string Source
        {
            get { return source; }
            set { source = value; }
        }

        [Flag("D", "Destination path")]
        public static string Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        [Flag("SC", "Source connection string")]
        public static string SourceConnection
        {
            get { return sourceConnection; }
            set { sourceConnection = value; }
        }

        [Flag("DC", "Destination connection string (default: local storage)")]
        public static string DestinationConnection
        {
            get { return destinationConnection; }
            set { destinationConnection = value; }
        }

        [Flag("Z", "Use GZip compression")]
        public static bool UseGZip
        {
            get { return useGZip; }
            set { useGZip = value; }
        }

        [Flag("P", "Make container public")]
        public static bool SetPublic
        {
            get { return setPublic; }
            set { setPublic = value; }
        }

        [Flag("L", "List blobs in container")]
        public static bool List
        {
            get { return list; }
            set { list = value; }
        }

        [Flag("R", "Remove container or directory")]
        public static bool Remove
        {
            get { return remove; }
            set { remove = value; }
        }

        [Flag("T", "Degree of parallelism for parallel uploading (default: 2)")]
        public static int Parallelism
        {
            get { return parallelism; }
            set { parallelism = value; }
        }

        [Flag("RC", "Retry max count (default: 10)")]
        public static int RetryCount
        {
            get { return retryCount; }
            set { retryCount = value; }
        }

        [Flag("RI", "Interval between retries in milliseconds (default: 5000)")]
        public static int RetryInterval
        {
            get { return retryInterval; }
            set { retryInterval = value; }
        }

        [Flag("BS", "Upload block size in bytes (default: 5242880 -> 5 MB)")]
        public static int BlockSize
        {
            get { return blockSize; }
            set { blockSize = value; }
        }
    }
}
