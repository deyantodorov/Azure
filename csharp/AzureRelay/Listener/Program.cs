using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Relay;

namespace Listener
{
    public class Program
    {
        private const string RelayNamespace = "mydtrelay.servicebus.windows.net";
        private const string ConnectionName = "checkconnection";
        private const string KeyName = "RootManageSharedAccessKey";
        private const string Key = "94rHzaDD3DxI1OYxB6O2mKF959pmrb0/01yQAOOIZGg=";

        private static async Task Main()
        {
            Console.WriteLine("Starting status check listener.");
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            // Create the listener
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var listener = new HybridConnectionListener(new Uri($"sb://{RelayNamespace}/{ConnectionName}"), tokenProvider);

            // Subscribe to the status events
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Check service is offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Check service is online"); };

            // Create an array of credit status values
            var statuses = new List<string>
            {
                "Good",
                "Some issues",
                "Bad"
            };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                // Obtain the name from the request
                var reader = new StreamReader(context.Request.InputStream);
                var requestedName = reader.ReadToEnd();
                Console.WriteLine("A request was received to check status for: " + requestedName);

                // Pick a status at random
                Random r = new Random();
                var index = r.Next(statuses.Count);

                // Formulate and send the response
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "Status check successful";

                using(var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.WriteLine($"Status check for {requestedName}: {statuses[index]}");
                }

                // Close the context
                context.Response.Close();
            };

            // Open the listener
            await listener.OpenAsync();
            Console.WriteLine("Server listening");

            // Start a new thread that will continuously read the console.
            await Console.In.ReadLineAsync();

            // Close the listener after you exit the processing loop.
            await listener.CloseAsync();
        }
    }
}
