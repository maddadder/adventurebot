using AdventureBot.Models;
using AdventureBot.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

[assembly: FunctionsStartup(typeof(AdventureBot.Startup))]
namespace AdventureBot
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var existingConfig = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(existingConfig)
                .AddEnvironmentVariables();

            var builtConfig = configBuilder.Build();
            var appConfigConnectionString = builtConfig["AppConfigurationConnectionString"];

            configBuilder.AddAzureAppConfiguration(options =>
                options
                    .Connect(appConfigConnectionString)                    
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(new DefaultAzureCredential());
                    })
            );
            builtConfig = configBuilder.Build();

            builder.Services.Replace(new ServiceDescriptor(typeof(IConfiguration), builtConfig));
            
            builder.Services.AddLogging();

            // config the strongly typed section
            builder.Services.Configure<AwsSesApiConfig>(builtConfig.GetSection("AwsSes"));
            builder.Services.Configure<ApplicationConfig>(builtConfig.GetSection("App"));
            builder.Services.Configure<DiscordConfig>(builtConfig.GetSection("Discord"));
            builder.Services.Configure<GraphApiAppConfig>(builtConfig.GetSection("GraphApiApp"));

            builder.Services.AddSingleton<ICosmosApiService, CosmosApiService>();
            builder.Services.AddSingleton<IAwsSesApiService, AwsSesApiService>();
            builder.Services.AddSingleton<IDiscordBotService, DiscordBotService>();

            builder.Services.AddSingleton<IGraphClientService, GraphClientService>();

            builder.Services.AddSingleton((s) => 
            {
                var dbConfig = builtConfig.GetSection("App");
                var conn = dbConfig[DbStrings.CosmosDBConnection];
                var client = new CosmosClient(conn);
                return client;
            });
        }
    }
}
