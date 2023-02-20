﻿using AdventureBot.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Azure.Identity;

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

            // config the strongly typed section
            builder.Services.Configure<ApplicationConfig>(builtConfig.GetSection("App"));
        }
    }
}