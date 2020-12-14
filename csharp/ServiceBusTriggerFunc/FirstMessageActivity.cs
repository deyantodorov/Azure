using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using Microsoft.Extensions.Logging;

namespace EventProcessing
{
    public class FirstMessageActivity
    {
        private readonly HttpClient _client;

        public FirstMessageActivity (IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
        }

        [FunctionName(nameof(FirstMessageActivity))]
        public async Task<MyMessage> Run ([ActivityTrigger] MyMessage message,
                               ILogger logger)
        {

            var response = await _client.GetAsync("https://www.google.com");

            if (response.IsSuccessStatusCode)
            {
                var newMsg = new MyMessage
                {
                    Name = "1 " + message.Name,
                    Email = "1 " + message.Email,
                    Content = await response.Content.ReadAsStringAsync(),
                };

                logger.LogInformation($"Content length is {newMsg.Content.Length}");

                return await Task.Run(() => newMsg);
            }

            var content = await response.Content.ReadAsStringAsync();
            throw new FunctionFailedException(content);
        }
    }
}