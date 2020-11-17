using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;

namespace AzureSamples.EventHub.Publisher
{
    public class Program
    {
        private const int eventsCount = 100;

        private static IConfigurationRoot configuration;

        private static async Task Main()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            await using var producerClient = new EventHubProducerClient(
                configuration.GetSection("connectionString").Value,
                configuration.GetSection("eventHubName").Value);

            using var eventBatch = await producerClient.CreateBatchAsync();

            for (int i = 1; i <= eventsCount; i++)
            {
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"Message {i}")));
            }

            await producerClient.SendAsync(eventBatch);

            Console.WriteLine($"A batch of {eventsCount} events has been  published.");
        }
    }
}
