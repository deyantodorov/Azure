using System.Text;
using System;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;

namespace EventProcessing
{
    public class SendNotificationActivity
    {
        [FunctionName(nameof(SendNotificationActivity))]
        public async Task Run (
            [ActivityTrigger] MyMessage message,
            [SendGrid(ApiKey = "SendGrid.MailSendKey")] IAsyncCollector<SendGridMessage> messageCollector,
            ILogger logger)
        {
            logger.LogInformation($"{message.Content.Length}");

            var newMsg = new SendGridMessage();

            newMsg.AddTo("name@mail.com");
            newMsg.SetFrom("alerts@xasa.com");
            newMsg.SetSubject("Something new...");
            var content = "<p>Bruce, our planet in is severe danger!</p>" +
              "<p>You are the only one who can stop a giant asteroid (see attachment). Please nuke it now!</p>" +
              "<p>Best regards, Humanity</p>";
            newMsg.AddContent(MimeType.Html, content);
            var attachment = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)));
            newMsg.AddAttachment($"{message.Name.Replace(" ", string.Empty)}.json", attachment);

            await messageCollector.AddAsync(newMsg);
        }
    }
}