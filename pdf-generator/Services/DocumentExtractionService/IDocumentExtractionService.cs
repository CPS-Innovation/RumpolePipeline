using System.Threading.Tasks;

namespace pdf_generator.Services.DocumentExtractionService
{
    public interface IDocumentExtractionService
    {
        Task<string> GetDocumentSasLinkAsync(int caseId, int documentId);
    }
}
