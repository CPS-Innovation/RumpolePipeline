using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CoordinatorStart
    {
        [FunctionName("CoordinatorStart")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}")] HttpRequestMessage req, string caseId, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
        {
            if (!int.TryParse(caseId, out var caseIdNum))
            {
                throw new ArgumentException("Invalid case id. A 32-bit integer is required.", caseId);
            }

            var instanceId = caseId;

            // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated).
            var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
            if (existingInstance == null
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
            {
                var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
                var force = query.Get("force");

                var forceRefresh = false;
                if(force != null && !bool.TryParse(force, out forceRefresh))
                {
                    throw new ArgumentException("Invalid query string. Force value must be a boolean.", force);
                }

                await orchestrationClient.StartNewAsync(
                    nameof(CoordinatorOrchestrator),
                    instanceId,
                    new CoordinatorOrchestrationPayload
                    {
                        CaseId = caseIdNum,
                        ForceRefresh = forceRefresh
                    });
            }

            log.LogInformation($"Started {nameof(CoordinatorOrchestrator)} with instance id '{instanceId}'");

            return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
