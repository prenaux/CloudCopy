namespace CloudCopy.Tests.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CloudCopy.Azure;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    [TestClass]
    public class UploaderTest
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
        public void ShouldUploadMultipleFiles()
        {
            var uploader = new Uploader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var basePath = Path.GetFullPath(folder + "\\..");

            uploader.UploadFiles(basePath, filesList, TestConnection, TestContainer, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });

            var directory = container.GetDirectoryReference(folder);
            Assert.IsNotNull(directory);

            var blobs = directory.ListBlobs();
            Assert.AreEqual(filesList.Count, blobs.Count());
            
            container.Delete();
        }

        [TestMethod]
        public void ShouldUploadGZip()
        {
            var uploader = new Uploader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var basePath = Path.GetFullPath(folder + "\\..");

            ArgSettings.UseGZip = true;

            uploader.UploadFiles(basePath, filesList, TestConnection, TestContainer, new System.Threading.CancellationToken(), (t) => { }, (t, e) => { }, () => { });

            var directory = container.GetDirectoryReference(folder);
            Assert.IsNotNull(directory);

            var blobs = directory.ListBlobs();
            Assert.AreEqual(filesList.Count, blobs.Count());

            container.Delete();
        }

        [TestMethod]
        public void ShouldReportOnEachUploadedFile()
        {
            var uploader = new Uploader();
            var storageAccount = CloudStorageAccount.DevelopmentStorageAccount;
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(TestContainer);
            var folder = "Pictures";
            var filesList = new DirectoryInfo(folder).GetFiles().Select(i => i.FullName).ToList();
            var loggedFiles = new List<string>();

            uploader.UploadFiles(string.Empty, filesList, TestConnection, TestContainer, new System.Threading.CancellationToken(), (t) => { loggedFiles.Add(t); }, (t, e) => { }, () => { });

            var blobs = container.ListBlobs();
            Assert.AreEqual(filesList.Count, loggedFiles.Count());

            foreach (var filePath in filesList)
            {
                Assert.IsTrue(loggedFiles.Contains(Path.GetFileName(filePath)));
            }

            container.Delete();
        }
    }
}
