using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using common.Domain.Exceptions;
using coordinator.Domain;
using coordinator.Handlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class CoordinatorStart
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<CoordinatorStart> _log;

        public CoordinatorStart(IExceptionHandler exceptionHandler, ILogger<CoordinatorStart> log)
        {
            _exceptionHandler = exceptionHandler;
            _log = log;
        }

        [FunctionName("CoordinatorStart")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}")] HttpRequestMessage req, string caseId, [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            try
            {
                if (!req.Headers.TryGetValues("Authorization", out var values) ||
                    string.IsNullOrWhiteSpace(values.FirstOrDefault()))
                {
                    throw new UnauthorizedException("No authorization token supplied.");
                }

                if (!int.TryParse(caseId, out var caseIdNum))
                {
                    throw new BadRequestException("Invalid case id. A 32-bit integer is required.", caseId);
                }

                var query = HttpUtility.ParseQueryString(req.RequestUri.Query);
                var force = query.Get("force");

                var forceRefresh = false;
                if (force != null && !bool.TryParse(force, out forceRefresh))
                {
                    throw new BadRequestException("Invalid query string. Force value must be a boolean.", force);
                }

                var accessToken = values.First().Replace("Bearer ", "");
                var instanceId = caseId;

                // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated/cancelled).
                var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
                if (existingInstance == null
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Canceled)
                {
                    await orchestrationClient.StartNewAsync(
                        nameof(CoordinatorOrchestrator),
                        instanceId,
                        new CoordinatorOrchestrationPayload
                        {
                            CaseId = caseIdNum,
                            ForceRefresh = forceRefresh,
                            AccessToken = accessToken
                        });

                    _log.LogInformation($"Started {nameof(CoordinatorOrchestrator)} with instance id '{instanceId}'.");
                }

                return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
            }
            catch(Exception exception)
            {
                return _exceptionHandler.HandleException(exception);
            }
        }
    }
}
