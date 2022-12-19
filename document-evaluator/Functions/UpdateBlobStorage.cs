using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Queues.Models;
using Common.Constants;
using Common.Domain.QueueItems;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace document_evaluator.Functions;

public class UpdateBlobStorage
{
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IValidatorWrapper<UpdateBlobStorageQueueItem> _validatorWrapper;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IConfiguration _configuration;
    
    public UpdateBlobStorage(IJsonConvertWrapper jsonConvertWrapper, IValidatorWrapper<UpdateBlobStorageQueueItem> validatorWrapper, 
        IConfiguration configuration, IBlobStorageService blobStorageService)
    {
        _jsonConvertWrapper = jsonConvertWrapper;
        _validatorWrapper = validatorWrapper;
        _blobStorageService = blobStorageService;
        _configuration = configuration;
    }
    
    [FunctionName("update-blob-storage")]
    public async Task RunAsync([QueueTrigger("update-blob-storage")] QueueMessage message, ILogger log)
    {
        log.LogInformation("Received message from {QueueName}, content={Content}", _configuration[ConfigKeys.SharedKeys.UpdateBlobStorageQueueName], message.MessageText);
        
        var request = _jsonConvertWrapper.DeserializeObject<UpdateBlobStorageQueueItem>(message.MessageText);
        var results = _validatorWrapper.Validate(request);
        if (results.Any())
            throw new Exception(string.Join(Environment.NewLine, results));
        
        log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"Beginning blob storage update for: {message.MessageText}");

        await _blobStorageService.RemoveDocumentAsync(request.BlobName, request.CorrelationId);
        
        log.LogMethodFlow(request.CorrelationId, nameof(RunAsync), $"Blob storage update completed for: {message.MessageText}");
    }
}
