using System;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;

namespace StorageServicePrinciple
{
    public static class Program
    {
        private static async Task Main(string[] args)
        {
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var tokenCredential = new TokenCredential(await azureServiceTokenProvider.GetAccessTokenAsync("https://storage.azure.com/"));

            var storageCredentials = new StorageCredentials(tokenCredential);

            try
            {
                var cloudStorageAccount = new CloudStorageAccount(
                    storageCredentials,
                    useHttps: true,
                    accountName: "storage1995339491",
                    endpointSuffix: "core.windows.net");

                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                var cref = cloudBlobClient.GetContainerReference("contribapp");

                await cref.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
