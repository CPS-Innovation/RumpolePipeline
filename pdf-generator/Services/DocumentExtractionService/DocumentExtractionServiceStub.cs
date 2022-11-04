using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.DocumentExtractionService
{
    [ExcludeFromCodeCoverage]
	public class DocumentExtractionServiceStub : IDocumentExtractionService
	{
        private readonly string _blobStorageConnectionString;
        private readonly ILogger<DocumentExtractionServiceStub> _logger;
        private readonly IConfiguration _configuration;

        public DocumentExtractionServiceStub(string blobStorageConnectionString, ILogger<DocumentExtractionServiceStub> logger, IConfiguration configuration)
		{
            _blobStorageConnectionString = blobStorageConnectionString;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"DocumentId: {documentId}, FileName: {fileName}");
            var blobContainerName = _configuration[ConfigKeys.PdfGeneratorKeys.FakeCmsDocumentsRepository];
            var useEndToEnd = bool.Parse(_configuration[FeatureFlags.EvaluateDocuments]);
            if (useEndToEnd)
                blobContainerName = _configuration[ConfigKeys.PdfGeneratorKeys.FakeCmsDocumentsRepository2];
            
            var blobClient = new BlobClient(_blobStorageConnectionString, blobContainerName, fileName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var blob = await blobClient.DownloadContentAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return blob.Value.Content.ToStream();
        }
    }
}

