using System;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, string documentId, string lastUpdatedDate, Guid correlationId);

        Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId);
    }
}
