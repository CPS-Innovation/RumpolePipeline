using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Common.Domain.DocumentEvaluation;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string materialId, string lastUpdatedDate, Guid correlationId);

        Task<List<TaggedBlobItemWrapper>> ListDocumentsForCaseAsync(string caseId, Guid correlationId);

        Task<TaggedBlobItemWrapper> FindDocumentForCaseAsync(string caseId, string documentId, Guid correlationId);

        Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId);
    }
}
