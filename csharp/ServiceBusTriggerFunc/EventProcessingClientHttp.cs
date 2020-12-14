using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;

using Microsoft.Extensions.Logging;

namespace EventProcessing
{

    public class EventProcessingClientHttp
    {
        [FunctionName(nameof(EventProcessingClientHttp))]
        public async Task<HttpResponseMessage> Run ([HttpTrigger(AuthorizationLevel.Function, "POST", Route = "start")] HttpRequestMessage message,
                                                    [DurableClient] IDurableClient orchestrationClient,
                                                    ILogger logger)
        {

            var detectedEvent = await message.Content.ReadAsAsync<MyMessage>();
            var messages = await orchestrationClient.StartNewAsync(nameof(EventProcessingOrchestrator), detectedEvent);

            logger.LogInformation($"HTTP started orchestration with ID {messages}.");

            return orchestrationClient.CreateCheckStatusResponse(message, messages);
        }
    }
}