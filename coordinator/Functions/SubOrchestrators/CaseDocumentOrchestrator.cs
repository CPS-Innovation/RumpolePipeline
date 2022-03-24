using System;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Requests;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace coordinator.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestrator
    {
        private readonly FunctionEndpointOptions _functionEndpoints;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        public CaseDocumentOrchestrator(IOptions<FunctionEndpointOptions> functionEndpointOptions, IJsonConvertWrapper jsonConvertWrapper)
        {
            _functionEndpoints = functionEndpointOptions.Value;
            _jsonConvertWrapper = jsonConvertWrapper;
        }

        [FunctionName("CaseDocumentOrchestrator")]
        public async Task RunDocumentOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
                if (payload == null)
                {
                    throw new ArgumentException("Orchestration payload cannot be null.", nameof(CaseDocumentOrchestrationPayload));
                }

                var tracker = GetTracker(context, payload.CaseId);

                //TODO how to add token to this
                var response = await CallHttpAsync<GeneratePdfResponse>(
                    context,
                    HttpMethod.Post,
                    _functionEndpoints.GeneratePdf,
                    new GeneratePdfRequest
                    {
                        CaseId = payload.CaseId,
                        DocumentId = payload.DocumentId
                    });

                await tracker.RegisterPdfBlobName(new RegisterPdfBlobNameArg { DocumentId = payload.DocumentId, BlobName = response.BlobName });
            }
            catch (Exception exception)
            {
                log.LogError(exception, $"Error when running {nameof(CaseDocumentOrchestrator)} orchestration.");
                throw;
            }
        }

        private async Task<T> CallHttpAsync<T>(IDurableOrchestrationContext context, HttpMethod httpMethod, string url, object content)
        {
            var response = await context.CallHttpAsync(httpMethod, new Uri(url), _jsonConvertWrapper.SerializeObject(content));
            return _jsonConvertWrapper.DeserializeObject<T>(response.Content);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}
