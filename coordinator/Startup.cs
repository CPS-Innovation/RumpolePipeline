using System;
using System.Diagnostics.CodeAnalysis;
using Common.Adapters;
using Common.Constants;
using common.Handlers;
using common.Wrappers;
using coordinator;
using coordinator.Clients;
using coordinator.Factories;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

[assembly: FunctionsStartup(typeof(Startup))]
namespace coordinator
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddTransient<IDocumentExtractionClient, DocumentExtractionClientStub>();
            builder.Services.AddSingleton(_ =>
            {
                const string instance = AuthenticationKeys.AzureAuthenticationInstanceUrl;
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
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IIdentityClientAdapter, IdentityClientAdapter>();
            builder.Services.AddTransient<IDefaultAzureCredentialFactory, DefaultAzureCredentialFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddSingleton<IGeneratePdfHttpRequestFactory, GeneratePdfHttpRequestFactory>();
            builder.Services.AddSingleton<ITextExtractorHttpRequestFactory, TextExtractorHttpRequestFactory>();
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