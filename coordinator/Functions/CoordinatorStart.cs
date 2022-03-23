﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
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

        public CoordinatorStart(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        [FunctionName("CoordinatorStart")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "cases/{caseId}")] HttpRequestMessage req, string caseId, [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
        {
            try
            {
                if (!req.Headers.TryGetValues("Authorization", out var values) || string.IsNullOrWhiteSpace(values.FirstOrDefault()))
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
                    throw new ArgumentException("Invalid query string. Force value must be a boolean.", force);
                }

                var accessToken = values.First().Replace("Bearer ", "");
                var instanceId = caseId;

                // Check if an instance with the specified ID already exists or an existing one stopped running(completed/failed/terminated).
                var existingInstance = await orchestrationClient.GetStatusAsync(caseId);
                if (existingInstance == null
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed
                || existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)
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
                }

                log.LogInformation($"Started {nameof(CoordinatorOrchestrator)} with instance id '{instanceId}'");

                return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
            }
            catch(Exception exception)
            {
                return _exceptionHandler.HandleException(exception);
            }
        }
    }
}
