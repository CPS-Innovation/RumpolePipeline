// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=HandlePolarisDocumentDeleted

using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Common.Constants;
using Common.Domain.QueueItems;
using Common.Logging;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions;

public class HandleDocumentDeletedEvent
{
    private readonly ILogger<HandleDocumentDeletedEvent> _logger;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IConfiguration _configuration;
    private readonly IStorageQueueService _storageQueueService;
    
    public HandleDocumentDeletedEvent(ILogger<HandleDocumentDeletedEvent> logger, IJsonConvertWrapper jsonConvertWrapper, 
        IConfiguration configuration, IStorageQueueService storageQueueService)
    {
        _logger = logger;
        _jsonConvertWrapper = jsonConvertWrapper;
        _configuration = configuration;
        _storageQueueService = storageQueueService;
    }

    /// <summary>
    /// Handles blob-deletion events raised by Azure Storage, when case documents grow stale either by lack of interaction with the CMS case or the CMS case is archived/closed
    /// The initial deletion is carried out via a LifeCycle Management policy setup against the storage table containing the documents 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    [FunctionName("HandleDocumentDeletedEvent")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
    {
        const string loggerSource = "HandleDocumentDeletedEvent - EventGrid - Event";
        var correlationId = Guid.NewGuid();

        var processCompleted = false;
        var response = string.Empty;
        var events = await BinaryData.FromStreamAsync(req.Body);
        _logger.LogMethodEntry(correlationId, loggerSource, $"Received events: {events}");

        try
        {
            var eventGridEvents = EventGridEvent.ParseMany(events);

            foreach (var eventGridEvent in eventGridEvents)
            {
                // Handle system events
                if (!eventGridEvent.TryGetSystemEventData(out var eventData)) continue;

                switch (eventData)
                {
                    // Handle the subscription validation event
                    case SubscriptionValidationEventData subscriptionValidationEventData:
                        _logger.LogMethodFlow(correlationId, loggerSource, 
                            $"Got SubscriptionValidation event data, validation code: {subscriptionValidationEventData.ValidationCode}, topic: {eventGridEvent.Topic}");
                        
                        var responseData = new
                        {
                            ValidationResponse = subscriptionValidationEventData.ValidationCode
                        };
                        return new OkObjectResult(responseData);
                    // Handle the storage blob created event
                    case StorageBlobDeletedEventData storageBlobDeletedEventData:
                        _logger.LogMethodFlow(correlationId, loggerSource, ReturnEventGridEventLevel(storageBlobDeletedEventData));

                        var blobDetails = new Uri(storageBlobDeletedEventData.Url).PathAndQuery.Split("/");
                        var caseId = long.Parse(blobDetails[2]);
                        var blobName = blobDetails[4];
                
                        await _storageQueueService.AddNewMessageAsync(_jsonConvertWrapper.SerializeObject(new UpdateSearchIndexByBlobNameQueueItem(caseId, 
                            blobName, correlationId)), _configuration[ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName]);
                
                        var searchIndexUpdated = $"The search index update was queued and should remove any joint references to caseId: {caseId} and blobName: '{blobName}'";
                        _logger.LogMethodFlow(correlationId, loggerSource, searchIndexUpdated);
                        break;
                }
            }

            processCompleted = true;
            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, loggerSource, ex.Message, ex);
            throw;
        }
        finally
        {
            _logger.LogMethodExit(correlationId, loggerSource, $"Blob deletion event completed successfully: '{processCompleted}'");
        }
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