using System;
using common.Wrappers;
using coordinator.Clients;
using coordinator.Factories;
using coordinator.Handlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

[assembly: FunctionsStartup(typeof(ServerlessPDFConversionDemo.Startup))]
namespace ServerlessPDFConversionDemo
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddTransient<IDocumentExtractionClient, DocumentExtractionClientStub>();
            builder.Services.AddTransient<IOnBehalfOfTokenClient, OnBehalfOfTokenClient>();
            builder.Services.AddSingleton(serviceProvider =>
            {
                var instance = "https://login.microsoftonline.com/";
                var onBehalfOfTokenTenantId = GetValueFromConfig(configuration, "OnBehalfOfTokenTenantId");
                var onBehalfOfTokenClientId = GetValueFromConfig(configuration, "OnBehalfOfTokenClientId");
                var onBehalfOfTokenClientSecret = GetValueFromConfig(configuration, "OnBehalfOfTokenClientSecret");
                var appOptions = new ConfidentialClientApplicationOptions
                {
                    Instance = instance,
                    TenantId = onBehalfOfTokenTenantId,
                    ClientId = onBehalfOfTokenClientId,
                    ClientSecret = onBehalfOfTokenClientSecret
                };

                var authority = $"{instance}{onBehalfOfTokenTenantId}/";

                return ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(appOptions).WithAuthority(authority).Build();
            });
            builder.Services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
        }

        private string GetValueFromConfig(IConfiguration configuration, string key)
        {
            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new Exception($"Value cannot be null: {key}");
            }

            return value;
        }
    }
}