using System;
using System.Linq;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Storage.Blobs;
using common.Handlers;
using common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using pdf_generator.Domain.Requests;
using pdf_generator.Factories;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.PdfService;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .Build();

            builder.Services.AddHttpClient<IDocumentExtractionService, DocumentExtractionService>(client =>
            {
                client.BaseAddress = new Uri(configuration["DocumentExtractionBaseUrl"]);
                client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            });

            builder.Services.AddSingleton<IPdfService, WordsPdfService>();
            builder.Services.AddSingleton<IPdfService, CellsPdfService>();
            builder.Services.AddSingleton<IPdfService, SlidesPdfService>();
            builder.Services.AddSingleton<IPdfService, ImagingPdfService>();
            builder.Services.AddSingleton<IPdfService, DiagramPdfService>();
            builder.Services.AddSingleton<IPdfService, HtmlPdfService>();
            builder.Services.AddSingleton<IPdfService, EmailPdfService>();
            builder.Services.AddSingleton<IPdfOrchestratorService, PdfOrchestratorService>(provider =>
            {
                var pdfServices = provider.GetServices<IPdfService>();
                var wordsPdfService = pdfServices.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = pdfServices.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = pdfServices.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = pdfServices.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = pdfServices.First(s => s.GetType() == typeof(DiagramPdfService));
                var htmlPdfService = pdfServices.First(s => s.GetType() == typeof(HtmlPdfService));
                var emailPdfService = pdfServices.First(s => s.GetType() == typeof(EmailPdfService));
                return new PdfOrchestratorService(
                    wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService, diagramPdfService, htmlPdfService, emailPdfService);
            });

            builder.Services.AddTransient<IValidatorWrapper<GeneratePdfRequest>, ValidatorWrapper<GeneratePdfRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IDocumentExtractionHttpRequestFactory, DocumentExtractionHttpRequestFactory>();
            builder.Services.AddTransient<IAuthorizationHandler>(_ =>
            {
                return new AuthorizationHandler(configuration["AuthorizationClaim"]);
            });
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();

            builder.Services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient(new Uri(configuration["BlobServiceUrl"]))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                return new BlobStorageService(
                    serviceProvider.GetRequiredService<BlobServiceClient>(),
                    configuration["BlobServiceContainerName"]);
            });
            builder.Services.AddTransient<IDocumentExtractionService>(serviceProvider =>
            {
                return new DocumentExtractionServiceStub(
                    configuration["StubBlobStorageConnectionString"]);
            });
        }
    }
}