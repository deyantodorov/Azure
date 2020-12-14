using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace EventProcessing
{
    public class EventProcessingClientServicebus
    {
        [FunctionName(nameof(EventProcessingClientServicebus))]
        public async Task Run ([ServiceBusTrigger("%TopicName%", "%SubscriptionName%", Connection = "SERVICEBUS_CONNECTION")] MyMessage message,
                               [DurableClient] IDurableClient orchestrationClient,
                               ILogger log)
        {
            var instanceId = await orchestrationClient.StartNewAsync("EventProcessingOrchestrator", message);

            log.LogInformation($"C# ServiceBus topic trigger function processed message: {message}");
        }
    }
}
