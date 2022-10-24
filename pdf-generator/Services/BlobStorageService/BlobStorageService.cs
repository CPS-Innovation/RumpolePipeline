using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
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
            _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            if (!await blobClient.ExistsAsync())
                return null;
            
            var blob = await blobClient.DownloadContentAsync();

            _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
            return blob.Value.Content.ToStream();
        }

        public async Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string materialId, string lastUpdatedDate, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(UploadDocumentAsync), blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(stream, true);
            stream.Close();

            var tags = new Dictionary<string, string>
            {
                {DocumentTags.CaseId, caseId},
                {DocumentTags.DocumentId, documentId},
                {DocumentTags.MaterialId, materialId},
                {DocumentTags.LastUpdatedDate, lastUpdatedDate}
            };
            await blobClient.SetTagsAsync(tags);

            _logger.LogMethodExit(correlationId, nameof(UploadDocumentAsync), string.Empty);
        }

        public async Task<List<TaggedBlobItemWrapper>> ListDocumentsForCaseAsync(string caseId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(ListDocumentsForCaseAsync), caseId);
            var documentsFound = new List<TaggedBlobItemWrapper>();
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var searchQuery = $"@container = '{_blobServiceContainerName}' AND \"{DocumentTags.CaseId}\" = '{caseId}'";
            await foreach (var page in blobContainerClient.FindBlobsByTagsAsync(searchQuery).AsPages())
            {
                foreach (var pageValue in page.Values)
                {
                    var newWrapper = new TaggedBlobItemWrapper
                    {
                        BlobItemTags = new Dictionary<string, string>(),
                        BlobItemName = pageValue.BlobName,
                        BlobItemContainerName = pageValue.BlobContainerName
                    };

                    foreach (var item in pageValue.Tags)
                    {
                        newWrapper.BlobItemTags.Add(item.Key, item.Value);
                    }
                    
                    documentsFound.Add(newWrapper);
                }
            }

            _logger.LogMethodExit(correlationId, nameof(ListDocumentsForCaseAsync), documentsFound.ToJson());
            return documentsFound;
        }
        
        public async Task<TaggedBlobItemWrapper> FindDocumentForCaseAsync(string caseId, string documentId, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(FindDocumentForCaseAsync), caseId);
            var documentsFound = new List<TaggedBlobItemWrapper>();
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");

            var searchQuery = $"@container = '{_blobServiceContainerName}' AND \"{DocumentTags.CaseId}\" = '{caseId}' AND \"{DocumentTags.DocumentId}\" = '{documentId}'";
            await foreach (var page in blobContainerClient.FindBlobsByTagsAsync(searchQuery).AsPages())
            {
                foreach (var pageValue in page.Values)
                {
                    var newWrapper = new TaggedBlobItemWrapper
                    {
                        BlobItemTags = new Dictionary<string, string>(),
                        BlobItemName = pageValue.BlobName,
                        BlobItemContainerName = pageValue.BlobContainerName
                    };

                    foreach (var item in pageValue.Tags)
                    {
                        newWrapper.BlobItemTags.Add(item.Key, item.Value);
                    }
                    
                    documentsFound.Add(newWrapper);
                }
            }

            _logger.LogMethodExit(correlationId, nameof(FindDocumentForCaseAsync), documentsFound.ToJson());
            return documentsFound.FirstOrDefault();
        }
        
        public async Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId)
        {
            _logger.LogMethodEntry(correlationId, nameof(RemoveDocumentAsync), blobName);

            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_blobServiceContainerName);
            if (!await blobContainerClient.ExistsAsync())
                throw new RequestFailedException((int)HttpStatusCode.NotFound, $"Blob container '{_blobServiceContainerName}' does not exist");
            
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            try
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return true;
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode != (int) HttpStatusCode.NotFound) throw;
                if (e.RequestInformation.ExtendedErrorInformation == null ||
                    e.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound ||
                    e.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.ContainerNotFound)
                {
                    return false;
                }

                throw;
            }
            finally
            {
                _logger.LogMethodExit(correlationId, nameof(RemoveDocumentAsync), string.Empty);
            }
        }
    }
}