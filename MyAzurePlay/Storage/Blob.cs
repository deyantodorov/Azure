using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Storage
{
    public static class Blob
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=storage1279615957;AccountKey=zuzzVu+xdVGycVltbRbCN3yUUq5YEwOH7UGkAUWjCSV6FxdGc1d7ZVcJp8Jxzh0it7TEtUGqHzW+d+MONCYPTg==";
        private const string MyContainer = "mycontainer";

        public static async Task RunAsync()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);

            var cloudBlobClient = storageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(MyContainer);

            await cloudBlobContainer.CreateIfNotExistsAsync();

            var permissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob,
            };

            await cloudBlobContainer.SetPermissionsAsync(permissions);

            var localFile = "data/Blob.txt";

            await File.WriteAllTextAsync(localFile, "Hello World!");

            var cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFile);

            await cloudBlockBlob.UploadFromFileAsync(localFile);

            Console.WriteLine("Show blobs in container.");

            BlobContinuationToken blobContinuationToken = null;

            do
            {
                var result = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);

                blobContinuationToken = result.ContinuationToken;

                foreach (var blobItem in result.Results)
                {
                    Console.WriteLine(blobItem.Uri);
                }

            } while (blobContinuationToken != null);

            var destinationFile = localFile.Replace(".txt", "_DOWNLOADED.txt");

            await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);

            var leaseId = Guid.NewGuid().ToString();

            await File.WriteAllTextAsync(localFile, "New Content");

            cloudBlockBlob.AcquireLease(TimeSpan.FromSeconds(30), leaseId);

            try
            {
                await cloudBlockBlob.UploadFromFileAsync(localFile);
            }
            catch (StorageException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5));

            await cloudBlockBlob.UploadFromFileAsync(localFile);

            await cloudBlockBlob.ReleaseLeaseAsync(new AccessCondition()
            {
                LeaseId = leaseId,
            });

            await cloudBlobContainer.DeleteIfExistsAsync();
        }
    }
}
