using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using Common.Domain.Responses;

namespace Common.Services.DocumentEvaluationService.Contracts;

public interface IDocumentEvaluationService
{
    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequest request, Guid correlationId);

    Task<List<EvaluateExistingDocumentResponse>> EvaluateExistingDocumentsAsync(long caseId, IEnumerable<CaseDocument> incomingDocuments, Guid correlationId);
}
