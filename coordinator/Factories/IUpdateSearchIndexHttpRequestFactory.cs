using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories;

public interface IUpdateSearchIndexHttpRequestFactory
{
    Task<DurableHttpRequest> Create(long caseId, string documentId, Guid correlationId);
}
