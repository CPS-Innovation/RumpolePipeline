
using System;
using common.Wrappers;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Handlers;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
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
            //TODO add all config to terraform
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddOptions<FunctionEndpointOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("functionEndpoints").Bind(setttings);
            });

            builder.Services.AddTransient<ICoreDataApiClient, CoreDataApiClient>();
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
            builder.Services.AddSingleton<IGraphQLClient>(s => new GraphQLHttpClient(GetValueFromConfig(configuration, "CoreDataApiUrl"), new NewtonsoftJsonSerializer()));
            builder.Services.AddTransient<IAuthenticatedGraphQLHttpRequestFactory, AuthenticatedGraphQLHttpRequestFactory>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
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