
using common.Wrappers;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Factories;
using coordinator.Handlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ServerlessPDFConversionDemo.Startup))]
namespace ServerlessPDFConversionDemo
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<EndpointOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("endpoint").Bind(setttings);
            });

            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<ICoreDataApiClient, CoreDataApiClient>();
            builder.Services.AddTransient<IOnBehalfOfTokenClient, OnBehalfOfTokenClient>();
            builder.Services.AddTransient<IAuthenticatedGraphQLHttpRequestFactory, AuthenticatedGraphQLHttpRequestFactory>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
        }
    }
}