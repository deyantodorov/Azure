using System;
using System.Text;
using System.Threading.Tasks;

using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace AzureSamples.EventHub.Publisher
{
    public class Program
    {
        private const int eventsCount = 100;
        private const string connectionString = "";
        private const string eventHubName = "";

        private static async Task Main()
        {
            await using var producerClient = new EventHubProducerClient(connectionString, eventHubName);

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
