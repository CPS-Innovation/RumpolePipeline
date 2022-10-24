using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories;

public interface IEvaluateExistingDocumentsHttpRequestFactory
{
    Task<DurableHttpRequest> Create(int caseId, List<CaseDocument> incomingDocuments, Guid correlationId);
}