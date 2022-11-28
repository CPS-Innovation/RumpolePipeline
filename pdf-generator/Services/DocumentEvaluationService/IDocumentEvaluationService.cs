using System;
using System.Threading.Tasks;
using Common.Domain.Requests;
using Common.Domain.Responses;

namespace pdf_generator.Services.DocumentEvaluationService;

public interface IDocumentEvaluationService
{
    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequest request, Guid correlationId);
}
