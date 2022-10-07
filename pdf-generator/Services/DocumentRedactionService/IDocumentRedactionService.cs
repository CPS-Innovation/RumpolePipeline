using System;
using System.Threading.Tasks;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken, Guid correlationId);
    }
}
