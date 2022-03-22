using System;
using System.Linq;
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
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

            var actualRoot = localRoot ?? azureRoot;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(actualRoot)
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddOptions<BlobStorageOptions>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("blobStorage").Bind(settings);
            });

            builder.Services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>(client =>
            {
                //TODO config to terraform
                client.BaseAddress = new Uri(configuration["DocumentExtractionBaseUrl"]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddTransient<IPdfService, WordsPdfService>();
            builder.Services.AddTransient<IPdfService, CellsPdfService>();
            builder.Services.AddTransient<IPdfService, SlidesPdfService>();
            builder.Services.AddTransient<IPdfService, ImagingPdfService>();
            builder.Services.AddTransient<IPdfService, DiagramPdfService>();
            builder.Services.AddTransient<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var pdfServices = provider.GetServices<IPdfService>();
                var wordsPdfService = pdfServices.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = pdfServices.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = pdfServices.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = pdfServices.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = pdfServices.First(s => s.GetType() == typeof(DiagramPdfService));
                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService, diagramPdfService);
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