namespace CloudCopy.Helpers
{
    using System.IO;
    using System.IO.Compression;

    public static class GZipHelper
    {
        public static Stream Zip(string filePath)
        {
            byte[] fileBytes;
            using (var tempStream = new FileStream(filePath, FileMode.Open))
            {
                fileBytes = new byte[tempStream.Length];
                tempStream.Read(fileBytes, 0, (int)tempStream.Length);
            }

            MemoryStream compressedStream;

            using (var tempStream = new MemoryStream())
            {
                using (var gz = new GZipStream(tempStream, CompressionMode.Compress, false))
                {
                    gz.Write(fileBytes, 0, fileBytes.Length);
                }

                compressedStream = new MemoryStream(tempStream.ToArray());
            }

            return compressedStream;
        }

        public static void Unzip(Stream stream, string destPath)
        {
            using (Stream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            using (Stream zipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                byte[] buffer = new byte[4096];
                int nRead;
                while ((nRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, nRead);
                }
            }
        }
    }
}
