using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Management.WebSites;
using Microsoft.Azure.Management.WebSites.Models;
using Microsoft.Rest.Azure.Authentication;

namespace ConfigureLoggingByServicePrincipal
{
    public static class Program
    {
        private static async Task Main()
        {
            var rg = "dt-az-play";
            var website = "amazon1761906289-app";

            var clientId = "01cf37a3-8812-4f8a-bd9e-26b91dcb7d9c";
            var clientSecret = "c2e81cde-eb85-4fd0-b9fc-bac42c02380f";
            var subscriptionId = "2b42e51d-a716-4758-bb96-a705b65b1e34";
            var tenantId = "3307b31f-9671-4473-88de-c624f8c2661e";
            var sasUrl = "https://stgacc1328538807.blob.core.windows.net/logs?sv=2018-03-28&si=logpolicy&sr=c&sig=EKL5LoxzdtuJRt14uACgHZGqQSZpl8keLmpKfjq%2BXKA%3D";

            var serviceCredentials = await ApplicationTokenProvider.LoginSilentAsync(tenantId, clientId, clientSecret);

            var client = new WebSiteManagementClient(serviceCredentials)
            {
                SubscriptionId = subscriptionId
            };

            var appSettings = new StringDictionary(
                name: "properties",
                properties: new Dictionary<string, string>()
                {
                    { "DIAGNOSTICS_AZUREBLOBCONTAINERSASURL", sasUrl },
                    { "DIAGNOSTICS_AZUREBLOBRETENTIONINDAYS", "30" },
                });

            await client.WebApps.UpdateApplicationSettingsAsync(resourceGroupName: rg, name: website, appSettings: appSettings);
        }
    }
}
