using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Domain;
using coordinator.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace coordinator.Functions
{
    public class CoordinatorOrchestrator
    {
        private readonly EndpointOptions _endpoints;

        public CoordinatorOrchestrator(IOptions<EndpointOptions> endpointOptions)
        {
            _endpoints = endpointOptions.Value;
        }

        [FunctionName("CoordinatorOrchestrator")]
        public async Task<List<TrackerDocument>> RunCaseOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var payload = context.GetInput<CoordinatorOrchestrationPayload>();

            if (payload == null)
            {
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(CoordinatorOrchestrationPayload));
            }
            var caseId = arg.CaseId;
            var transactionId = context.InstanceId;
            var tracker = GetTracker(context, caseId);

            //if (!arg.ForceRefresh && await tracker.GetIsAlreadyProcessed())
            //{
            //    return await tracker.GetDocuments();
            //}

            await tracker.Initialise(transactionId);

            var cmsCaseDocumentDetails = await CallHttpAsync<List<CmsCaseDocumentDetails>>(context, HttpMethod.Get, _endpoints.CmsDocumentDetails);
            await tracker.Register(cmsCaseDocumentDetails.Select(item => item.Id).ToList());

            var caseDocumentTasks = new List<Task<string>>();
            foreach (var caseDocumentDetails in cmsCaseDocumentDetails)
            {
                // kick off the processing of each document in parallel
                caseDocumentDetails.CaseId = caseId;
                caseDocumentDetails.TransactionId = transactionId;
                caseDocumentTasks.Add(context.CallSubOrchestratorAsync<string>("CaseDocumentOrchestration", caseDocumentDetails));
            }

            await Task.WhenAll(caseDocumentTasks);

            tracker.RegisterIsIndexed();

            return await tracker.GetDocuments();
        }

        [FunctionName("CaseDocumentOrchestration")]
        public async Task RunDocumentOrchestrator(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var caseDocument = context.GetInput<CmsCaseDocumentDetails>();
            var caseId = caseDocument.CaseId;
            var documentId = caseDocument.Id;
            var transactionId = caseDocument.TransactionId;

            var tracker = GetTracker(context, caseId);

            // convert doc to pdf
            var pdfBlobNameAndSasLinkUrl = await CallHttpAsync<BlobNameAndSasLinkUrl>(context, HttpMethod.Post, _endpoints.DocToPdf, new DocToPdfArg
            {
                CaseId = caseId,
                DocumentId = documentId,
                DocumentUrl = caseDocument.Url,
                TransactionId = transactionId
            });
            tracker.RegisterPdfUrl(new TrackerPdfArg
            {
                DocumentId = documentId,
                PdfUrl = pdfBlobNameAndSasLinkUrl.SasLinkUrl
            });

            // ocr
            await context.CallSubOrchestratorAsync("CaseDocumentSearchPdfOrchestration", new PdfToSearchDataArg
            {
                CaseId = caseId,
                DocumentId = documentId,
                SasLink = pdfBlobNameAndSasLinkUrl.SasLinkUrl,
                TransactionId = transactionId
            });
        }

        [FunctionName("CaseDocumentSearchPdfOrchestration")]
        public async Task RunDocumentSearchPdf(
        [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var arg = context.GetInput<PdfToSearchDataArg>();

            var response = await CallHttpAsync<List<PdfToSearchDataResponse>>(context, HttpMethod.Post, _endpoints.PdfToSearchData, arg);
            var tracker = GetTracker(context, arg.CaseId);

            tracker.RegisterIsProcessedForSearchAndPageDimensions(new TrackerPageArg
            {
                DocumentId = arg.DocumentId,
                PageDimensions = response
                                .OrderBy(item => item.PageIndex)
                                .Select(item => new TrackerPageDimensions
                                {
                                    Height = item.Height,
                                    Width = item.Width
                                }).ToList()
            });
        }

        private async Task<T> CallHttpAsync<T>(IDurableOrchestrationContext context, HttpMethod httpMethod, string url, object arg = null)
        {
            var response = await context.CallHttpAsync(httpMethod, new Uri(url), arg == null ? null : JsonConvert.SerializeObject(arg));
            return JsonConvert.DeserializeObject<T>(response.Content);
        }

        private ITracker GetTracker(IDurableOrchestrationContext context, int caseId)
        {
            var entityId = new EntityId(nameof(Tracker), caseId.ToString());
            return context.CreateEntityProxy<ITracker>(entityId);
        }
    }
}