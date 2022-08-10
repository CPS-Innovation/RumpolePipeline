using System;
using System.Threading.Tasks;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;

namespace pdf_generator.Services.DocumentRedactionService
{
    internal class DocumentRedactionService : IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdf(RedactPdfRequest redactPdfRequest, string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
