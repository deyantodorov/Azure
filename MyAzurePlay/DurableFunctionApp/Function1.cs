using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctionApp
{


    public static class Function1
    {
        [FunctionName("Conductor")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>
            {
                await context.CallActivityAsync<string>("CalculateTax", "10"),
                await context.CallActivityAsync<string>("CalculateTax", "100"),
                await context.CallActivityAsync<string>("CalculateTax", "1000")
            };

            return outputs;
        }

        [FunctionName("CalculateTax")]
        public static double CalculateTax([ActivityTrigger] string value, ILogger log)
        {
            var output = double.Parse(value) * 1.13;
            return output;
        }

        [FunctionName("Starter")]
        public static async Task<HttpResponseMessage> Starter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Conductor", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}