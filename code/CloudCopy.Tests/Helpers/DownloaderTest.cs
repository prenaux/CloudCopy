namespace CloudCopy.Tests.Helpers
{
    using System.IO;
    using System.Linq;
    using CloudCopy.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    [TestClass]
    public class DownloaderTest
    {
        private const string TestContainer = "cloudcopy-test";
        private const string TestConnection = "UseDevelopmentStorage=true";

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            if (System.Diagnostics.Process.GetProcessesByName("DSService").Length == 0)
            {
                System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();

                start.Arguments = "/devstore:start";
                start.FileName = @"C:\Program Files\Windows Azure SDK\v1.4\bin\csrun.exe";

                var proc = new System.Diagnostics.Process();
                proc.StartInfo = start;
                proc.Start();
                proc.WaitForExit();
            }
        }

        [TestMethod]
        public void ShouldDowloadMultipleFilesWithBaseDirectory()
        {
            var downloader = new Downloader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            
            this.UploadTestFiles();

            var baseDirectory = "http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures/";
            var destFolder = "Downloaded";
            var blobs = (new BlobsListing()).ListBlobs(baseDirectory, "*.png", TestConnection, true);

            downloader.DownloadFiles(baseDirectory, blobs, destFolder, TestConnection, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });

            container.Delete();

            var dirInfo = new DirectoryInfo(destFolder);
            Assert.AreEqual(blobs.Count, dirInfo.GetFiles().Length - 1); // -1 txt file
        }

        [TestMethod]
        public void ShouldDowloadMultipleFilesWithNoBaseDirectory()
        {
            var downloader = new Downloader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);

            this.UploadTestFiles();

            var baseDirectory = "http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures/";
            var destFolder = "Downloaded";
            var blobs = (new BlobsListing()).ListBlobs(baseDirectory, "*.png", TestConnection, true);

            downloader.DownloadFiles(string.Empty, blobs, destFolder, TestConnection, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });

            container.Delete();

            var dirInfo = new DirectoryInfo(destFolder);
            Assert.AreEqual(blobs.Count, dirInfo.GetFiles().Length - 1); // -1 txt file
        }

        [TestMethod]
        public void ShouldDowloadMultipleFilesWithNoBaseDirectoryGZip()
        {
            var downloader = new Downloader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            
            ArgSettings.UseGZip = true;

            this.UploadTestFiles();

            var baseDirectory = "http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures/";
            var destFolder = "Downloaded";
            var blobs = (new BlobsListing()).ListBlobs(baseDirectory, "*.txt", TestConnection, true);

            downloader.DownloadFiles(string.Empty, blobs, destFolder, TestConnection, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });

            container.Delete();

            var dirInfo = new DirectoryInfo(destFolder);
            Assert.AreEqual(blobs.Count, dirInfo.GetFiles().Length);
        }

        private void UploadTestFiles()
        {
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var uploader = new Uploader();
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var basePath = Path.GetFullPath(folder + "\\..");

            uploader.UploadFiles(basePath, filesList, TestConnection, TestContainer, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });
        }
    }
}
