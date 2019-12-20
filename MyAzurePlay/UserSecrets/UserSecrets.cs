using Microsoft.Extensions.Configuration;

namespace Common
{
    /// <summary>
    /// Use Secret Manager Tool
    /// </summary>
    /// <example>
    /// https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=windows
    /// </example>
    public class UserSecrets
    {
        public IConfigurationRoot Configuration { get; private set; }

        public UserSecrets()
        {
            var builder = new ConfigurationBuilder();

            builder.AddUserSecrets<UserSecrets>();

            Configuration = builder.Build();
        }
    }
}
