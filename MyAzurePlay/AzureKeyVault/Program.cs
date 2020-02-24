using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace AzureKeyVault
{
    public static class Program
    {
        private static async Task Main()
        {
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();

            var kvc = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var kvBaseUrl = "https://keyvault8627981.vault.azure.net/";

            var secret = await kvc.GetSecretAsync(kvBaseUrl, "connectionString");

            Console.WriteLine(secret);
        }
    }
}
