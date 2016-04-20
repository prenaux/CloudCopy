namespace CloudCopy.Helpers
{
    using System.IO;
    using Microsoft.Win32;

    public class MimeTypeHelper
    {
        public static string MimeType(string Filename)
        {
            var mime = "application/octetstream";
            var ext = Path.GetExtension(Filename).ToLower();
            var key = Registry.ClassesRoot.OpenSubKey(ext);

            if (key != null && key.GetValue("Content Type") != null)
            {
                mime = key.GetValue("Content Type").ToString();
            }

            return mime;
        } 
    }
}
