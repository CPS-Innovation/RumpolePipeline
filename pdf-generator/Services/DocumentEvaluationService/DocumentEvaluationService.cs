using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Logging;
using Microsoft.Extensions.Logging;
using pdf_generator.Services.BlobStorageService;

namespace pdf_generator.Services.DocumentEvaluationService;

public class DocumentEvaluationService : IDocumentEvaluationService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentEvaluationService> _logger;
    
    public DocumentEvaluationService(IBlobStorageService blobStorageService, ILogger<DocumentEvaluationService> logger)
    {
        _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// If any stored documents are not found inside the list of incoming documents retrieved from CMS, then those documents must have been removed from CMS by a CMS operative
    /// Any such documents should be removed from storage and the search index
    /// </summary>
    /// <param name="caseId"></param>
    /// <param name="incomingDocuments"></param>
    /// <param name="correlationId"></param>
    public async Task<List<EvaluateDocumentResponse>> EvaluateExistingDocumentsAsync(string caseId, List<CaseDocument> incomingDocuments, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(EvaluateExistingDocumentsAsync), caseId);
        var response = new List<EvaluateDocumentResponse>();

        var currentlyStoredDocuments = await _blobStorageService.ListDocumentsForCaseAsync(caseId, correlationId);
        if (currentlyStoredDocuments.Count == 0)
            return response;

        foreach (var storedDocument in currentlyStoredDocuments)
        {
            var storedDocumentId = storedDocument.BlobItemTags[DocumentTags.DocumentId];
            var storedMaterialId = storedDocument.BlobItemTags[DocumentTags.MaterialId];
            var storedLastUpdatedDate = storedDocument.BlobItemTags[DocumentTags.LastUpdatedDate];

            var storedDocumentInCms = incomingDocuments.Any(incomingDocument => incomingDocument.DocumentId == storedDocumentId && incomingDocument.MaterialId == storedMaterialId && incomingDocument.LastUpdatedDate == storedLastUpdatedDate);

            if (!storedDocumentInCms)
            {
                await _blobStorageService.RemoveDocumentAsync(storedDocument.BlobItemName, correlationId);
                
                response.Add(new EvaluateDocumentResponse
                {
                    CaseId = caseId,
                    DocumentId = storedDocumentId,
                    UpdateSearchIndex = true
                });
            }
        }

        _logger.LogMethodExit(correlationId, nameof(EvaluateExistingDocumentsAsync), response.ToJson());
        return response;
    }

    /// <summary>
    /// Attempt to find the incoming CMS document's details in the documents already stored for the case
    /// </summary>
    /// <param name="request"></param>
    /// <param name="correlationId"></param>
    /// <returns></returns>
    public async Task<EvaluateDocumentResponse> EvaluateDocumentAsync(EvaluateDocumentRequest request, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(EvaluateDocumentAsync), request.ToJson());
        var response = new EvaluateDocumentResponse
        {
            CaseId = request.CaseId.ToString(),
            DocumentId = request.DocumentId
        };

        var currentlyStoredDocument = await _blobStorageService.FindDocumentForCaseAsync(request.CaseId.ToString(), request.DocumentId, correlationId);
        if (currentlyStoredDocument == null)
        {
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = false;
            return response;
        }

        var storedMaterialId = currentlyStoredDocument.BlobItemTags[DocumentTags.MaterialId];
        var storedLastUpdatedDate = currentlyStoredDocument.BlobItemTags[DocumentTags.LastUpdatedDate];

        if (request.MaterialId == storedMaterialId && request.LastUpdatedDate == storedLastUpdatedDate)
        {
            response.EvaluationResult = DocumentEvaluationResult.DocumentUnchanged;
            response.UpdateSearchIndex = false;
        }
        else
        {
            await _blobStorageService.RemoveDocumentAsync(currentlyStoredDocument.BlobItemName, correlationId);
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = true;
        }
        
        return response;
    }
}
