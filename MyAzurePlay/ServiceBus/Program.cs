using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace ServiceBus
{
    public static class Program
    {
        private static async Task Main()
        {
            await RunAsync();
        }

        private static async Task RunAsync()
        {
            const string serviceBusConnectionString = "Endpoint=sb://name845614277.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=s99t/DtswfKHaS8g70RRla0wand4MzvomHpb7aRMrC0=";
            const string queueName = "testqueue";
            const int delay = 2000;
            const int numMessageToSend = 10;

            var queueClient = new QueueClient(serviceBusConnectionString, queueName);

            // register the function that will process messages
            queueClient.RegisterMessageHandler(async (message, cancellationToken) =>
            {
                Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

                if (delay > 0)
                {
                    await Task.Delay(delay);
                }

                await queueClient.CompleteAsync(message.SystemProperties.LockToken);
            },
                new MessageHandlerOptions(exception =>
                {
                    Console.WriteLine($"Message handler encountered an exception {exception.Exception}.");
                    var context = exception.ExceptionReceivedContext;
                    Console.WriteLine($"- Endpoint: {context.Endpoint}");
                    Console.WriteLine($"- Entity Path: {context.EntityPath}");
                    Console.WriteLine($"- Executing Action: {context.Action}");
                    return Task.CompletedTask;
                })
                {
                    MaxConcurrentCalls = 5,
                    AutoComplete = false
                }
                );

            for (var i = 0; i < numMessageToSend; i++)
            {
                var messageBody = $"Message {i}";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                Console.WriteLine($"Sending message: {messageBody}");

                await queueClient.SendAsync(message);
            }

            Task.Delay(30000).Wait();

            await queueClient.CloseAsync();
        }
    }
}
