using Microsoft.Azure.Functions.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using EventProcessing;


[assembly: FunctionsStartup(typeof(Startup))]
namespace EventProcessing
{
    public class Startup : FunctionsStartup
    {
        public override void Configure (IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
        }
    }
}
