using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluationService.Contracts;
using Microsoft.Extensions.Logging;

namespace Common.Services.DocumentEvaluationService;

public class DocumentEvaluationService : IDocumentEvaluationService
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<DocumentEvaluationService> _logger;
    
    public DocumentEvaluationService(IBlobStorageService blobStorageService, ILogger<DocumentEvaluationService> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
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
        EvaluateDocumentResponse response;
        
        var blobSearchResult = await _blobStorageService.FindBlobsByPrefixAsync(request.ProposedBlobName, correlationId);
        var blobInfo = blobSearchResult.FirstOrDefault();

        if (blobInfo == null)
        {
            response = new EvaluateDocumentResponse(request.CaseId, request.DocumentId, request.VersionId, false, DocumentEvaluationResult.AcquireDocument);
            return response;
        }
        
        if (request.VersionId == blobInfo.VersionId)
        {
            response = new EvaluateDocumentResponse(request.CaseId, request.DocumentId, request.VersionId, false, DocumentEvaluationResult.DocumentUnchanged);
        }
        else
        {
            await _blobStorageService.RemoveDocumentAsync(request.ProposedBlobName, correlationId);
            response = new EvaluateDocumentResponse(request.CaseId, request.DocumentId, request.VersionId, true, DocumentEvaluationResult.AcquireDocument);
        }
        
        return response;
    }
    
    /// <summary>
    /// If any stored documents are not found inside the list of incoming documents retrieved from CMS, then those documents must have been removed from CMS by a CMS operative
    /// Any such documents should be removed from storage and the search index
    /// </summary>
    /// <param name="caseId"></param>
    /// <param name="incomingDocuments"></param>
    /// <param name="correlationId"></param>
    public async Task<List<EvaluateExistingDocumentResponse>> EvaluateExistingDocumentsAsync(long caseId, IEnumerable<CaseDocument> incomingDocuments, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(EvaluateExistingDocumentsAsync), caseId.ToString());
        var response = new List<EvaluateExistingDocumentResponse>();
        
        var blobPrefix = $"{caseId}/pdfs";
        var currentlyConvertedDocuments = await _blobStorageService.FindBlobsByPrefixAsync(blobPrefix, correlationId);
        if (currentlyConvertedDocuments.Count == 0)
            return response;

        var patternsToExamine = incomingDocuments.Select(incomingDocument => 
            $"{caseId}/pdfs/{Path.GetFileNameWithoutExtension(incomingDocument.FileName)}_{incomingDocument.DocumentId}.pdf").ToList();

        foreach (var convertedDocument in currentlyConvertedDocuments
                     .Where(convertedDocument => 
                         !patternsToExamine.Exists(x => x.Equals(convertedDocument.BlobName, StringComparison.OrdinalIgnoreCase))))
        {
            await _blobStorageService.RemoveDocumentAsync(convertedDocument.BlobName, correlationId);

            response.Add(new EvaluateExistingDocumentResponse(caseId, convertedDocument.BlobName, true, DocumentEvaluationResult.DocumentRemovedInCms));
        }

        _logger.LogMethodExit(correlationId, nameof(EvaluateExistingDocumentsAsync), response.ToJson());
        return response;
    }
}
