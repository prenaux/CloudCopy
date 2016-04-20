namespace CloudCopy.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public static class HashHelper
    {
        public static string ComputeMD5(Stream source)
        {
            byte[] hash;
            source.Seek(0, SeekOrigin.Begin);

            using (var md5 = MD5.Create())
            {
                hash = md5.ComputeHash(source);
            }

            source.Seek(0, SeekOrigin.Begin);

            return Convert.ToBase64String(hash);
        }
    }
}
