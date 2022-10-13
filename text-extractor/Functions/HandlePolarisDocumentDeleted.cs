// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=HandlePolarisDocumentDeleted

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Common.Constants;
using common.Domain.Exceptions;
using common.Handlers;
using Common.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using text_extractor.Services.SearchIndexService;

namespace text_extractor.Functions;

public class HandlePolarisDocumentDeleted
{
    private readonly ILogger<HandlePolarisDocumentDeleted> _logger;
    private readonly ISearchIndexService _searchIndexService;
    private readonly IAuthorizationValidator _authorizationValidator;
    
    public HandlePolarisDocumentDeleted(IAuthorizationValidator authorizationValidator, ILogger<HandlePolarisDocumentDeleted> logger, ISearchIndexService searchIndexService)
    {
        _logger = logger;
        _searchIndexService = searchIndexService;
        _authorizationValidator = authorizationValidator;
    }

    /// <summary>
    /// Handles blob-deletion events raised by Azure Storage, when case documents grow stale either by lack of interaction with the CMS case or the CMS case is archived/closed
    /// The initial deletion is carried out via a LifeCycle Management policy setup against the storage table containing the documents 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    [FunctionName("HandlePolarisDocumentDeleted")]
    public async Task<HttpResponseMessage> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage request)
    {
        var processCompleted = false;
        const string loggerSource = "HandlePolarisDocumentDeleted - EventGrid - Event";
        var correlationId = Guid.NewGuid();

        try
        {
            var authValidation = await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, correlationId, PipelineScopes.HandlePolarisDocumentDeleted, PipelineRoles.HandlePolarisDocumentDeleted);
            if (!authValidation.Item1)
                throw new UnauthorizedException("Token validation failed");
            
            if (request.Content == null)
                throw new ArgumentNullException(nameof(request));

            var requestMessageContent = await request.Content.ReadAsStringAsync();
            var eventGridEvent = (JsonConvert.DeserializeObject<EventGridEvent[]>(requestMessageContent) ?? Array.Empty<EventGridEvent>()).FirstOrDefault();
            if (eventGridEvent == null || string.IsNullOrWhiteSpace(eventGridEvent.EventType))
            {
                throw new ArgumentNullException(nameof(eventGridEvent), "Null or invalid Event Grid Event received");
            }

            _logger.LogMethodEntry(correlationId, loggerSource, ReturnEventGridTopLevel(eventGridEvent));

            // Validate whether EventType is of "Microsoft.EventGrid.SubscriptionValidationEvent"
            if (string.Equals(eventGridEvent.EventType, EventGridEvents.SubscriptionValidationEvent, StringComparison.OrdinalIgnoreCase))
            {
                var eventData = eventGridEvent.Data.ToObjectFromJson<SubscriptionValidationEventData>();
                var responseData = new Common.Domain.Validation.SubscriptionValidationResponseData
                {
                    ValidationResponse = eventData.ValidationCode
                };

                return responseData.ValidationResponse != null 
                    ? request.CreateResponse(HttpStatusCode.OK, responseData) 
                    : request.CreateErrorResponse(HttpStatusCode.BadRequest, "Could not deal with the Event Grid subscription validation request");
            }
            
            if (eventGridEvent.EventType == EventGridEvents.BlobDeletedEvent)
            {
                var eventData = eventGridEvent.Data.ToObjectFromJson<StorageBlobDeletedEventData>();
                if (eventData == null)
                    throw new NullReferenceException("Could not deserialize event data into the expected type: 'StorageBlobDeletedEventData'");
        
                _logger.LogMethodFlow(correlationId, loggerSource, ReturnEventGridEventLevel(eventData));

                var blobDetails = new Uri(eventData.Url).PathAndQuery.Split("/");
                var caseId = int.Parse(blobDetails[2]);
                var documentId = blobDetails[4].Replace(".pdf", "", StringComparison.OrdinalIgnoreCase);

                await _searchIndexService.RemoveResultsForDocumentAsync(caseId, documentId, correlationId);
                
                _logger.LogMethodFlow(correlationId, loggerSource, $"Removed caseId: {caseId}, documentId: '{documentId}' from the search index");
            }
            else
            {
                var wrongMessageTypeReceived = $"Event grid event type was not of type Microsoft.Storage.BlobDeleted, but rather {eventGridEvent.EventType} - ignoring the raised event";
                _logger.LogMethodFlow(correlationId, loggerSource, wrongMessageTypeReceived);
            }
            
            processCompleted = true;
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, loggerSource, ex.Message, ex);
            return request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
        }
        finally
        {
            _logger.LogMethodExit(correlationId, loggerSource, $"Blob deletion event completed successfully: '{processCompleted}'");
        }
        
        return request.CreateResponse(HttpStatusCode.OK);
    }

    private static string ReturnEventGridTopLevel(EventGridEvent eventGridEvent)
    {
        return $@"New Event Grid Event:
            - Id=[{eventGridEvent.Id}]
            - EventType=[{eventGridEvent.EventType}]
            - EventTime=[{eventGridEvent.EventTime}]
            - Subject=[{eventGridEvent.Subject}]
            - Topic=[{eventGridEvent.Topic}]";
    }

    private static string ReturnEventGridEventLevel(StorageBlobDeletedEventData eventData)
    {
        return $@"Received {EventGridEvents.BlobDeletedEvent} Event: 
            - Api=[{eventData.Api}]
            - BlobType=[{eventData.BlobType}]
            - ClientRequestId=[{eventData.ClientRequestId}]
            - ContentType=[{eventData.ContentType}]
            - RequestId=[{eventData.RequestId}]
            - Sequencer=[{eventData.Sequencer}]
            - StorageDiagnostics=[{eventData.StorageDiagnostics.ToString()}]
            - Url=[{eventData.Url}]";
    }
}
