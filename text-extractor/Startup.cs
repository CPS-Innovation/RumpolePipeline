
using common.Handlers;
using common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using text_extractor.Domain.Requests;
using text_extractor.Handlers;
using text_extractor.Services.OcrService;
using text_extractor.Services.SearchIndexService;
using text_extractor.Services.SasGeneratorService;
using text_extractor.Factories;
using text_extractor.Wrappers;
using Azure.Identity;
using System;

[assembly: FunctionsStartup(typeof(text_extractor.Startup))]
namespace text_extractor
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddOptions<OcrOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("ocrService").Bind(setttings);
            });
            builder.Services.AddOptions<SearchIndexOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("searchIndexService").Bind(setttings);
            });
            builder.Services.AddOptions<BlobOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("blob").Bind(setttings);
            });
            builder.Services.AddSingleton<IOcrService, OcrService>();
            builder.Services.AddSingleton<ISearchIndexService, SearchIndexService>();
            builder.Services.AddTransient<ISasGeneratorService, SasGeneratorService>();
            builder.Services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(new Uri($"{configuration["BlobServiceClientUrl"]}"))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IAuthorizationHandler, AuthorizationHandler>();
            builder.Services.AddTransient<IValidatorWrapper<ExtractTextRequest>, ValidatorWrapper<ExtractTextRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            builder.Services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            builder.Services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            builder.Services.AddTransient<ISearchLineFactory, SearchLineFactory>();
            builder.Services.AddTransient<ISearchClientFactory, SearchClientFactory>();
            builder.Services.AddTransient<ISearchIndexingBufferedSenderFactory, SearchIndexingBufferedSenderFactory>();
        }
    }
}