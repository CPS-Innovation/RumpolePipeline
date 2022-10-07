using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace pdf_generator.Services.DocumentExtractionService
{
    [ExcludeFromCodeCoverage]
	public class DocumentExtractionServiceStub : IDocumentExtractionService
	{
        private readonly string _blobStorageConnectionString;
        private readonly ILogger<DocumentExtractionServiceStub> _logger;

        public DocumentExtractionServiceStub(string blobStorageConnectionString, ILogger<DocumentExtractionServiceStub> logger)
		{
            _blobStorageConnectionString = blobStorageConnectionString;
            _logger = logger;
        }

        public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"DocumentId: {documentId}, FileName: {fileName}");
            var blobClient = new BlobClient(_blobStorageConnectionString, "cms-documents", fileName);

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

