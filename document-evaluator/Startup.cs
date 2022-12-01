using System;
using System.Diagnostics.CodeAnalysis;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Domain.Requests;
using Common.Exceptions.Contracts;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Handlers;
using Common.Services.BlobStorageService;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluationService;
using Common.Services.DocumentEvaluationService.Contracts;
using Common.Services.SearchService;
using Common.Services.SearchService.Contracts;
using Common.Wrappers;
using document_evaluation.Domain.Handlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(document_evaluation.Startup))]
namespace document_evaluation
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
            
            builder.Services.AddSingleton<IConfiguration>(configuration);
            
            builder.Services.AddTransient<IValidatorWrapper<EvaluateExistingDocumentsRequest>, ValidatorWrapper<EvaluateExistingDocumentsRequest>>();
            builder.Services.AddTransient<IValidatorWrapper<EvaluateDocumentRequest>, ValidatorWrapper<EvaluateDocumentRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            
            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration[ConfigKeys.SharedKeys.BlobServiceUrl]))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<BlobStorageService>>();
                
                return new BlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                    configuration[ConfigKeys.SharedKeys.BlobServiceContainerName], loggingService);
            });

            builder.Services.AddTransient<IHttpRequestFactory, HttpRequestFactory>();

            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            builder.Services.AddTransient<ISearchServiceProcessor, SearchServiceProcessor>();
            builder.Services.AddTransient<ISearchService, SearchService>();
            builder.Services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
        }
    }
}
