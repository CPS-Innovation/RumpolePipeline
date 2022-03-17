using System;
using System.Net.Http;
using System.Threading.Tasks;
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
                //TODO
                //throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);
            }

            // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated).
            var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
            if (existingInstance == null
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
            || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
            {
                //var query = System.Web.HttpUtility.ParseQueryString(req.RequestUri.Query);
                //// pass ?force=... if we do not want to be given the existing cached results
                //var force = query.Get("force");

                await orchestrationClient.StartNewAsync(nameof(CoordinatorOrchestrator), caseId, new CoordinatorOrchestrationPayload
                {
                    CaseId = caseIdNum,
                    //TrackerUrl = $"{req.RequestUri.GetLeftPart(UriPartial.Path)}/tracker{req.RequestUri.Query}",
                    //ForceRefresh = force == "true"
                });
            }

            log.LogInformation($"Started {nameof(CoordinatorOrchestrator)} with instance id '{caseId}'");

            return orchestrationClient.CreateCheckStatusResponse(req, caseId);
        }
    }
}
