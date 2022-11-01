using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace pdf_generator.Services.BlobStorageService
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _blobServiceContainerName;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(BlobServiceClient blobServiceClient, string blobServiceContainerName, ILogger<BlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _blobServiceContainerName = blobServiceContainerName;
            _logger = logger;
        }

        public async Task<Stream> GetDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);
            if (!await blobClient.ExistsAsync())
                return null;
            
            var blob = await blobClient.DownloadContentAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return blob.Value.Content.ToStream();
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string lastUpdatedDate, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(UploadDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            await blobClient.UploadAsync(stream, true);
            stream.Close();

            var metadata = new Dictionary<string, string>
            {
                {DocumentTags.CaseId, caseId},
                {DocumentTags.DocumentId, documentId},
                {DocumentTags.LastUpdatedDate, lastUpdatedDate ?? DateTime.UtcNow.ToString("yyyy-MM-dd")}
            };

            await blobClient.SetMetadataAsync(metadata);

            _logger.LogMethodExit(correlationId, nameof(UploadDocumentAsync), string.Empty);
        }

        public async Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId)
        {
            var decodedBlobName = blobName.UrlDecodeString();
            _logger.LogMethodEntry(correlationId, nameof(RemoveDocumentAsync), decodedBlobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(decodedBlobName);

            try
            {
                var deleteResult = await blobClient.DeleteIfExistsAsync();
                _logger.LogMethodFlow(correlationId, nameof(RemoveDocumentAsync), deleteResult ? $"Blob '{decodedBlobName}' deleted successfully from '{_blobServiceContainerName}'" 
                    : $"Blob '{decodedBlobName}' deleted unsuccessfully from '{_blobServiceContainerName}'");
                return true;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode != (int) HttpStatusCode.NotFound) throw;

                if (e.RequestInformation.ExtendedErrorInformation != null && e.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
                    return true; //nothing to remove, probably because it is the first time for the case or the blob storage has undergone lifecycle management
                
                throw;
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(RemoveDocumentAsync), string.Empty);
            }
        }
    }
}