using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using pdf_generator.Domain.SearchResults;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<bool> DocumentExistsAsync(string blobName, Guid correlationId);

        Task<List<BlobSearchResult>> FindBlobsByPrefixAsync(string blobPrefix, Guid correlationId);
        
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string versionId, Guid correlationId);

        Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId);
    }
}
