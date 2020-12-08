using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.WebJobs;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Onboarding
{
    public class RegisterNewHireQueueTrigger
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [FunctionName("RegisterNewHireQueueTrigger")]
        public async Task Run (
            [QueueTrigger("newhire-queue", Connection = "AzureWebJobsStorage")] NewHire newHire,
            IBinder binder,
            ILogger log)
        {
            var subscriptionUri = "https://demo.azure-api.net/setup/subscription";
            var queryParams = new Dictionary<string, string> { { "name", newHire.Name } };
            var subscriptionUriWithQueryParams = QueryHelpers.AddQueryString(subscriptionUri, queryParams);
            var result = await _httpClient.PostAsync(subscriptionUriWithQueryParams, null);

            if (result.IsSuccessStatusCode)
            {
                var subscription = await result.Content.ReadAsAsync<JToken>();
                var dynamicBlobBinding = new BlobAttribute("subscriptions/{rand-guid}.json");

                using var writer = await binder.BindAsync<TextWriter>(dynamicBlobBinding);
                await writer.WriteAsync(subscription.ToString(Formatting.Indented));
            }
        }
    }
}
