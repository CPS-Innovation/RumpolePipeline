using System;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.DocumentExtractionService
{
    public interface IDocumentExtractionService
    {
        Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId);
    }
}
