using System;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Azure.Relay;

namespace Sender
{
    public class Program
    {
        // Details of the Azure Relay
        private const string RelayNamespace = "mydtrelay.servicebus.windows.net";
        private const string ConnectionName = "checkconnection";
        private const string KeyName = "RootManageSharedAccessKey";
        private const string Key = "94rHzaDD3DxI1OYxB6O2mKF959pmrb0/01yQAOOIZGg=";

        private static async Task Main()
        {
            Console.WriteLine("Starting credit check sender...");
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            // Get a name from the user
            Console.WriteLine("Enter a name to check:");
            var name = Console.ReadLine();

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var uri = new Uri($"https://{RelayNamespace}/{ConnectionName}");
            var token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;

            // Create an HttpClient and formulate the request
            var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = uri,
                Method = HttpMethod.Get,
                Content = new StringContent(name)
            };

            request.Headers.Add("ServiceBusAuthorization", token);

            // Send the request
            var response = await client.SendAsync(request);

            // Display the result
            Console.WriteLine(await response.Content.ReadAsStringAsync());

            // Wait for the user to press return
            Console.ReadLine();
        }
    }
}
