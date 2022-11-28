using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<bool> DocumentExistsAsync(string blobName, Guid correlationId);

        Task<List<string>> FindBlobsByPrefixAsync(string blobPrefix, Guid correlationId);
        
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string versionId, Guid correlationId);

        Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId);
    }
}
