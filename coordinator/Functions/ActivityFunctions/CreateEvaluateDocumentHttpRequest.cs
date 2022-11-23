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

public class CreateEvaluateDocumentHttpRequest
{
    private readonly IEvaluateDocumentHttpRequestFactory _evaluateDocumentHttpRequestFactory;
    private readonly ILogger<CreateEvaluateDocumentHttpRequest> _log;

    public CreateEvaluateDocumentHttpRequest(IEvaluateDocumentHttpRequestFactory evaluateDocumentHttpRequestFactory, ILogger<CreateEvaluateDocumentHttpRequest> logger)
    {
        _evaluateDocumentHttpRequestFactory = evaluateDocumentHttpRequestFactory;
        _log = logger;
    }
    
    [FunctionName("CreateEvaluateDocumentHttpRequest")]
    public async Task<DurableHttpRequest> Run([ActivityTrigger] IDurableActivityContext context)
    {
        const string loggingName = $"{nameof(CreateEvaluateDocumentHttpRequest)} - {nameof(Run)}";
        var payload = context.GetInput<CreateEvaluateDocumentHttpRequestActivityPayload>();
            
        if (payload == null)
            throw new ArgumentException("Payload cannot be null.");
        if (payload.CaseId == 0)
            throw new ArgumentException("CaseId cannot be zero");
        if (string.IsNullOrWhiteSpace(payload.DocumentId))
            throw new ArgumentException("DocumentId is empty");
        if (payload.CorrelationId == Guid.Empty)
            throw new ArgumentException("CorrelationId must be valid GUID");
            
        _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
        var result = await _evaluateDocumentHttpRequestFactory.Create(payload.CaseId, payload.DocumentId, payload.VersionId, payload.CorrelationId);
            
        _log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
        return result;
    }
}
