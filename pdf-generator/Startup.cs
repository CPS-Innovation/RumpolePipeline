using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Storage.Blobs;
using Common.Domain.Requests;
using Common.Handlers;
using Common.Wrappers;
using FluentValidation;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain.Validators;
using pdf_generator.Factories;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentEvaluationService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.PdfService;
using pdf_generator.Services.SearchService;

[assembly: FunctionsStartup(typeof(pdf_generator.Startup))]
namespace pdf_generator
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
                var servicesList = pdfServices.ToList();
                var wordsPdfService = servicesList.First(s => s.GetType() == typeof(WordsPdfService));
                var cellsPdfService = servicesList.First(s => s.GetType() == typeof(CellsPdfService));
                var slidesPdfService = servicesList.First(s => s.GetType() == typeof(SlidesPdfService));
                var imagingPdfService = servicesList.First(s => s.GetType() == typeof(ImagingPdfService));
                var diagramPdfService = servicesList.First(s => s.GetType() == typeof(DiagramPdfService));
                var htmlPdfService = servicesList.First(s => s.GetType() == typeof(HtmlPdfService));
                var emailPdfService = servicesList.First(s => s.GetType() == typeof(EmailPdfService));
                var loggingService = provider.GetService<ILogger<PdfOrchestratorService>>();

                return new PdfOrchestratorService(wordsPdfService, cellsPdfService, slidesPdfService, imagingPdfService, 
                    diagramPdfService, htmlPdfService, emailPdfService, loggingService);
            });

            builder.Services.AddTransient<ICoordinateCalculator, CoordinateCalculator>();
            builder.Services.AddTransient<IValidatorWrapper<GeneratePdfRequest>, ValidatorWrapper<GeneratePdfRequest>>();
            builder.Services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            builder.Services.AddTransient<IDocumentExtractionHttpRequestFactory, DocumentExtractionHttpRequestFactory>();
            builder.Services.AddTransient<IAuthorizationValidator, AuthorizationValidator>();
            builder.Services.AddTransient<IExceptionHandler, ExceptionHandler>();
            builder.Services.AddTransient<IAsposeItemFactory, AsposeItemFactory>();

            builder.Services.AddAzureClients(azureClientFactoryBuilder =>
            {
                azureClientFactoryBuilder.AddBlobServiceClient(new Uri(configuration["BlobServiceUrl"]))
                    .WithCredential(new DefaultAzureCredential());
            });
            builder.Services.AddTransient<IBlobStorageService>(serviceProvider =>
            {
                var loggingService = serviceProvider.GetService<ILogger<BlobStorageService>>();
                
                return new BlobStorageService(serviceProvider.GetRequiredService<BlobServiceClient>(),
                        configuration["BlobServiceContainerName"], loggingService);
            });
            builder.Services.AddTransient<IDocumentExtractionService>(extractionProvider =>
            {
                var loggingService = extractionProvider.GetService<ILogger<DocumentExtractionServiceStub>>();
                return new DocumentExtractionServiceStub(configuration["StubBlobStorageConnectionString"], loggingService);
            });
            builder.Services.AddTransient<IDocumentRedactionService, DocumentRedactionService>();
            builder.Services.AddTransient<IDocumentEvaluationService, DocumentEvaluationService>();
            builder.Services.AddScoped<IValidator<RedactPdfRequest>, RedactPdfRequestValidator>();
            builder.Services.AddTransient<ISearchServiceProcessor, SearchServiceProcessor>();
            builder.Services.AddTransient<ISearchService, SearchService>();
        }
    }
}