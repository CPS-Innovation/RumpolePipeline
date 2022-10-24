using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using Common.Domain.Responses;

namespace pdf_generator.Services.DocumentEvaluationService;

public interface IDocumentEvaluationService
{
    Task<List<EvaluateDocumentResponse>> EvaluateExistingDocumentsAsync(string caseId, List<CaseDocument> incomingDocuments, Guid correlationId);

    Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequest request, Guid correlationId);
}
