namespace CloudCopy.Tests.Helpers
{
    using System.IO;
    using System.Linq;
    using CloudCopy.Azure;
    using CloudCopy.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    [TestClass]
    public class BlobsListingTest
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
        public void ShouldListFilteredBlobsInDrectory()
        {
            var listing = new BlobsListing();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var basePath = Path.GetFullPath(folder + "\\..");

            this.UploadTestFiles();

            var blobs = listing.ListBlobs("http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures", "*.txt", TestConnection, true);

            Assert.AreEqual(1, blobs.Count());
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures/Notes.txt", blobs[0]);

            container.Delete();
        }

        [TestMethod]
        public void ShouldListFilteredBlobsInContainer()
        {
            var listing = new BlobsListing();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var basePath = Path.GetFullPath(folder + "\\..");

            this.UploadTestFiles();

            var blobs = listing.ListBlobs("http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test", "*.txt", TestConnection, true);

            Assert.AreEqual(1, blobs.Count());
            Assert.AreEqual("http://127.0.0.1:10000/devstoreaccount1/cloudcopy-test/Pictures/Notes.txt", blobs[0]);

            container.Delete();
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
