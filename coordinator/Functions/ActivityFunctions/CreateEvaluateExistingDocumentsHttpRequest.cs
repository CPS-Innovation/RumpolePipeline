using System;
using System.Threading.Tasks;
using Common.Domain.Extensions;
using Common.Logging;
using coordinator.Domain;
using coordinator.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions;

public class CreateEvaluateExistingDocumentsHttpRequest
{
    private readonly IEvaluateExistingDocumentsHttpRequestFactory _evaluateExistingDocumentsHttpRequestFactory;
    private readonly ILogger<CreateEvaluateExistingDocumentsHttpRequest> _log;

    public CreateEvaluateExistingDocumentsHttpRequest(IEvaluateExistingDocumentsHttpRequestFactory evaluateExistingDocumentsHttpRequestFactory, ILogger<CreateEvaluateExistingDocumentsHttpRequest> logger)
    {
        _evaluateExistingDocumentsHttpRequestFactory = evaluateExistingDocumentsHttpRequestFactory;
        _log = logger;
    }
    
    [FunctionName("CreateEvaluateExistingDocumentsHttpRequest")]
    public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
    {
        const string loggingName = $"{nameof(CreateEvaluateExistingDocumentsHttpRequest)} - {nameof(Run)}";
        var payload = context.GetInput<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>();
            
        if (payload == null)
            throw new ArgumentException("Payload cannot be null.");
        if (string.IsNullOrWhiteSpace(payload.CaseUrn))
            throw new ArgumentException("CaseUrn cannot be empty");
        if (payload.CaseId == 0)
            throw new ArgumentException("CaseId cannot be zero");
        if (payload.CaseDocuments == null || payload.CaseDocuments.Count == 0)
            throw new ArgumentException("No existing case documents to evaluate");
        if (payload.CorrelationId == Guid.Empty)
            throw new ArgumentException("CorrelationId must be valid GUID");
            
        _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
        var result = await _evaluateExistingDocumentsHttpRequestFactory.Create(payload.CaseId, payload.CaseDocuments, payload.CorrelationId);
            
        _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        return result;
    }
}
