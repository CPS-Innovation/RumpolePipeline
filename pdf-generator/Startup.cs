using System;
using System.Net.Http.Headers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Domain.Requests;
using pdf_generator.Factories;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.PdfService;
using pdf_generator.Wrappers;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageOptions>().Configure<IConfiguration>((setttings, configuration) =>
            {
                configuration.GetSection("blobStorage").Bind(setttings);
            });

            builder.Services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>(client =>
            {
                client.BaseAddress = new Uri("http://www.google.co.uk");
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddTransient<IPdfService, WordsPdfService>();
            builder.Services.AddTransient<IPdfService, CellsPdfService>();
            builder.Services.AddTransient<IPdfService, SlidesPdfService>();
            builder.Services.AddTransient<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var wordsPdfService = provider.GetRequiredService<WordsPdfService>();
                var cellsPdfService = provider.GetRequiredService<CellsPdfService>();
                var slidesPdfService = provider.GetRequiredService<SlidesPdfService>();
                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService);
            });

            builder.Services.AddTransient<IDocumentExtractionService, DocumentExtractionService>();
            builder.Services.AddTransient<IBlobStorageService, BlobStorageService>();

            builder.Services.AddTransient<IValidatorWrapper<GeneratePdfRequest>, ValidatorWrapper<GeneratePdfRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();

            builder.Services.AddTransient<IDocumentExtractionHttpRequestFactory, DocumentExtractionHttpRequestFactory>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
        }
    }
}