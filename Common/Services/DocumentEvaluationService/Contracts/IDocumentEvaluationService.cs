using System;
using System.Threading.Tasks;
using Common.Domain.Requests;
using Common.Domain.Responses;

namespace Common.Services.DocumentEvaluationService.Contracts;

public interface IDocumentEvaluationService
{
    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequest request, Guid correlationId);
}
