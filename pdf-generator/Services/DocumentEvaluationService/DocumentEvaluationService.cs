using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
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
        var response = new EvaluateDocumentResponse
        {
            CaseId = request.CaseId.ToString(),
            DocumentId = request.DocumentId
        };
        
        var blobSearchResult = await _blobStorageService.FindBlobsByPrefixAsync(request.ProposedBlobName, correlationId);
        var blobInfo = blobSearchResult.FirstOrDefault();

        if (blobInfo == null)
        {
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = false;
            return response;
        }
        
        if (request.VersionId == blobInfo.VersionId)
        {
            response.EvaluationResult = DocumentEvaluationResult.DocumentUnchanged;
            response.UpdateSearchIndex = false;
        }
        else
        {
            await _blobStorageService.RemoveDocumentAsync(request.ProposedBlobName, correlationId);
            response.EvaluationResult = DocumentEvaluationResult.AcquireDocument;
            response.UpdateSearchIndex = true;
        }
        
        return response;
    }
}
