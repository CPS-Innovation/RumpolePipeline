using System;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace Common.Services.Contracts;

public interface IDocumentExtractionService
{
    Task<CaseDocument[]> ListDocumentsAsync(string caseUrn, string caseId, string accessToken, Guid correlationId);
        
    Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentCategory, string documentId, string accessToken, Guid correlationId);

    Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId);
}