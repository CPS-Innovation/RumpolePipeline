using System.Threading.Tasks;
using System;
using System.Linq;
using Azure.Storage.Queues.Models;
using Common.Constants;
using Common.Domain.Requests;
using Common.Logging;
using Common.Services.DocumentEvaluationService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace document_evaluator.Functions;

public class EvaluateExistingDocuments
{
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IValidatorWrapper<EvaluateExistingDocumentsRequest> _validatorWrapper;
    private readonly IDocumentEvaluationService _documentEvaluationService;

    public EvaluateExistingDocuments(IJsonConvertWrapper jsonConvertWrapper, IValidatorWrapper<EvaluateExistingDocumentsRequest> validatorWrapper, IDocumentEvaluationService documentEvaluationService)
    {
        _jsonConvertWrapper = jsonConvertWrapper;
        _validatorWrapper = validatorWrapper;
        _documentEvaluationService = documentEvaluationService;
    }
    
    [FunctionName("evaluate-existing-documents")]
    public async Task RunAsync(
        [QueueTrigger("evaluate-existing-documents")] QueueMessage message,
        [Queue("update-search-index-by-blob-name")] ICollector<UpdateSearchIndexByBlobNameRequest> collector,
        ILogger log)
    {
        log.LogInformation("Received message from evaluate-existing-documents, content={Content}", message.MessageText);
        
        var request = _jsonConvertWrapper.DeserializeObject<EvaluateExistingDocumentsRequest>(message.MessageText);
        var results = _validatorWrapper.Validate(request);
        if (results.Any())
            throw new Exception(string.Join(Environment.NewLine, results));

        log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"Evaluating existing documents for '{request.CaseId}'");
        var evaluationResults = await _documentEvaluationService.EvaluateExistingDocumentsAsync(request.CaseId, request.CaseDocuments, request.CorrelationId);

        if (evaluationResults is {Count: > 0})
        {
            log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"{evaluationResults.Count} results found, where the document no longer exists in CMS - updating search index.");

            foreach (var evaluationResult in evaluationResults)
            {
                log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), "Dispatching message to queue: update-search-index");
                collector.Add(new UpdateSearchIndexByBlobNameRequest(evaluationResult.CaseId, evaluationResult.BlobName, request.CorrelationId));
            }
        }
        else
        {
            log.LogMethodExit(request.CorrelationId, nameof(RunAsync), $"No evaluation actions required for {request.CaseId}.");
        }
    }
}
