using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using common.Domain.Exceptions;
using common.Handlers;
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
        private readonly IAuthorizationValidator _authorizationValidator;

        public CoordinatorStart(IExceptionHandler exceptionHandler, ILogger<CoordinatorStart> log, IAuthorizationValidator authorizationValidator)
        {
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _authorizationValidator = authorizationValidator ?? throw new ArgumentNullException(nameof(authorizationValidator));
        }

        [FunctionName("CoordinatorStart")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}")] HttpRequestMessage req, string caseId, [DurableClient] IDurableOrchestrationClient orchestrationClient)
        {
            try
            {
                var authValidation = await _authorizationValidator.ValidateTokenAsync(req.Headers.Authorization);
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

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
                        caseId,
                        new CoordinatorOrchestrationPayload
                        {
                            CaseId = caseIdNum,
                            ForceRefresh = forceRefresh,
                            AccessToken = authValidation.Item2
                        });

                    _log.LogInformation($"Started {nameof(CoordinatorOrchestrator)} with instance id '{caseId}'.");
                }

                return orchestrationClient.CreateCheckStatusResponse(req, caseId);
            }
            catch(Exception exception)
            {
                return _exceptionHandler.HandleException(exception);
            }
        }
    }
}
