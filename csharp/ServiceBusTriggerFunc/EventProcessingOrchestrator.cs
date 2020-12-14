using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using Microsoft.Extensions.Logging;

namespace EventProcessing
{
    public class EventProcessingOrchestrator
    {
        [FunctionName(nameof(EventProcessingOrchestrator))]
        public async Task Run ([OrchestrationTrigger] IDurableOrchestrationContext context,
                               ILogger logger)
        {
            var message = context.GetInput<MyMessage>();

            var firstMessage = await context.CallActivityWithRetryAsync<MyMessage>(
                nameof(FirstMessageActivity),
                new RetryOptions(TimeSpan.FromSeconds(10), 5),
                message);

            var secondMessage = await context.CallActivityWithRetryAsync<MyMessage>(
                nameof(SecondMessageActivity),
                new RetryOptions(TimeSpan.FromSeconds(10), 5),
                message);

            var messages = new List<MyMessage>
            {
                firstMessage,
                secondMessage
            };

            foreach (var msg in messages)
            {
                logger.LogInformation($"{msg}");
            }

            await context.CallActivityAsync(nameof(StoreProcessedEventActivity), firstMessage);
            await context.CallActivityAsync(nameof(StoreProcessedEventActivity), secondMessage);

            await context.CallActivityAsync(nameof(SendNotificationActivity), firstMessage);
            await context.CallActivityAsync(nameof(SendNotificationActivity), secondMessage);
        }
    }
}
