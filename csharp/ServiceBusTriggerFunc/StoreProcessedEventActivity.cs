using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventProcessing
{
    public class StoreProcessedEventActivity
    {
        [FunctionName(nameof(StoreProcessedEventActivity))]
        public async Task Run (
            [ActivityTrigger] MyMessage message,
            IBinder binder,
            ILogger logger)
        {
            logger.LogInformation($"{message.Content.Length}");

            var blobPath = $"messages/{message.Name.Replace(" ", string.Empty)}.json";
            var dynamicBlobBinding = new BlobAttribute(blobPath: blobPath) { Connection = "ProcessedStorage" };

            using var writer = await binder.BindAsync<TextWriter>(dynamicBlobBinding);
            await writer.WriteAsync(JsonConvert.SerializeObject(message, Formatting.Indented));
        }
    }
}