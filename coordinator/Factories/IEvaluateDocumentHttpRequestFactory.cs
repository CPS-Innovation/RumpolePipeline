using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories;

public interface IEvaluateDocumentHttpRequestFactory
{
    Task<DurableHttpRequest> Create(long caseId, string documentId, long versionId, Guid correlationId);
}