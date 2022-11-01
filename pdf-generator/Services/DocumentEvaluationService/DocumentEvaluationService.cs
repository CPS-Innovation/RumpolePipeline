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
using pdf_generator.Services.SearchService;

namespace pdf_generator.Services.DocumentEvaluationService;

public class DocumentEvaluationService : IDocumentEvaluationService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ISearchService _searchService;
    private readonly ILogger<DocumentEvaluationService> _logger;
    
    public DocumentEvaluationService(IBlobStorageService blobStorageService, ISearchService searchService, ILogger<DocumentEvaluationService> logger)
    {
        _blobStorageService = blobStorageService;
        _searchService = searchService;
        _logger = logger;
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

        var currentlyStoredDocuments = await _searchService.ListDocumentsForCaseAsync(caseId, correlationId);
        if (currentlyStoredDocuments.Count == 0)
            return response;

        foreach (var storedDocument in currentlyStoredDocuments)
        {
            var storedDocumentId = storedDocument.DocumentMetadata[DocumentTags.DocumentId];
            var storedLastUpdatedDate = storedDocument.DocumentMetadata[DocumentTags.LastUpdatedDate];

            var storedDocumentInCms = incomingDocuments.Any(incomingDocument => incomingDocument.DocumentId == storedDocumentId
                                                                                && incomingDocument.LastUpdatedDate == storedLastUpdatedDate);

            if (storedDocumentInCms) continue;
            
            await _blobStorageService.RemoveDocumentAsync(storedDocument.BlobName, correlationId);
                
            response.Add(new EvaluateDocumentResponse
            {
                CaseId = caseId,
                DocumentId = storedDocumentId,
                UpdateSearchIndex = true
            });
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

        var currentlyStoredDocument = await _searchService.FindDocumentForCaseAsync(request.CaseId.ToString(), request.DocumentId, correlationId);
        if (currentlyStoredDocument == null)
        {
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = false;
            return response;
        }

        var storedLastUpdatedDate = currentlyStoredDocument.DocumentMetadata[DocumentTags.LastUpdatedDate];

        if (request.LastUpdatedDate == storedLastUpdatedDate)
        {
            response.EvaluationResult = DocumentEvaluationResult.DocumentUnchanged;
            response.UpdateSearchIndex = false;
        }
        else
        {
            await _blobStorageService.RemoveDocumentAsync(currentlyStoredDocument.BlobName, correlationId);
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = true;
        }
        
        return response;
    }
}
