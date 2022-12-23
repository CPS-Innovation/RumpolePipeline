using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.EventGrid;
using Common.Constants;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions;

public class HandleDocumentDeletedEventTests
{
    private readonly Fixture _fixture;
    private const string QueueName = "update-search-index-by-blob-name";
    private readonly Mock<IStorageQueueService> _mockStorageQueueService;
    private Mock<HttpRequest> _mockHttpRequest;

    private readonly HandleDocumentDeletedEvent _handleDocumentDeletedEvent;
    
    public HandleDocumentDeletedEventTests()
    {
        _fixture = new Fixture();
        _mockStorageQueueService = new Mock<IStorageQueueService>();

        var logger = new Mock<ILogger<HandleDocumentDeletedEvent>>();
        var mockJsonConverterWrapper = new Mock<IJsonConvertWrapper>();
        var mockConfiguration = new Mock<IConfiguration>();
    
        mockConfiguration.Setup(x => x[ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName]).Returns(QueueName);
        
        _handleDocumentDeletedEvent = new HandleDocumentDeletedEvent(logger.Object, mockJsonConverterWrapper.Object, mockConfiguration.Object, _mockStorageQueueService.Object);
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEvent_IsNull_ThrowNullReferenceException()
    {
        var act = async () =>
        {
           await _handleDocumentDeletedEvent.RunAsync(null);
        };

        using (new AssertionScope())
        {
            await act.Should().ThrowAsync<NullReferenceException>();
            _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
                Times.Never);
        }
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsBlobDeleted_AndEventDataIsReceivedAsExpected_ThenTheEventIsProcessed_UsingTheCorrectParams()
    {
        const string eventJson = @"[{
            ""topic"": ""/subscriptions/4ae5270e-9e32-4bdf-88f1-a677dd3280c0/resourceGroups/Storage/providers/Microsoft.Storage/storageAccounts/sacpsdevrumpolepipeline"",
            ""subject"": ""/blobServices/default/containers/documents/18848/pdfs/docCDE.pdf"",
            ""eventType"": ""Microsoft.Storage.BlobDeleted"",
            ""eventTime"": ""2017-11-07T20:09:22.5674003Z"",
            ""id"": ""4c2359fe-001e-00ba-0e04-58586806d298"",
            ""data"": {
                ""api"": ""DeleteBlob"",
                ""clientRequestId"": ""a2b52c16-3aab-4f42-4567-321eae73f697"",
                ""requestId"": ""810e5826-101e-0058-1834-df9e6f000000"",
                ""eTag"": ""0x8DAAD4B298B5F20"",
                ""contentType"": ""application/octet-stream"",
                ""contentLength"": 56754,
                ""blobType"": ""BlockBlob"",
                ""url"": ""https://sacpsdevrumpolepipeline.blob.core.windows.net/documents/18848/pdfs/docCDE.pdf"",
                ""sequencer"": ""00000000000000000000000000015EBD000000000008c2d5"",
                ""storageDiagnostics"": {
                    ""batchId"": ""c68eb2e3-a006-003a-0034-dfe775000000""
                }
            },
            ""dataVersion"": ""1"",
            ""metadataVersion"": ""1""
            }]";
        
        _mockHttpRequest = CreateMockRequest(eventJson);
        
        await _handleDocumentDeletedEvent.RunAsync(_mockHttpRequest.Object);
        
        _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
            Times.Once);
    }
    
    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsValidationEvent_AndEventDataIsReceivedAsExpected_ThenTheEventIsProcessed_UsingTheCorrectParams()
    {
        const string eventJson = @"[{
            ""topic"": ""/subscriptions/4ae5270e-9e32-4bdf-88f1-a677dd3280c0/resourceGroups/Storage/providers/Microsoft.Storage/storageAccounts/sacpsdevrumpolepipeline"",
            ""subject"": ""/blobServices/default/containers/documents/18848/pdfs/docCDE.pdf"",
            ""eventType"": ""Microsoft.EventGrid.SubscriptionValidationEvent"",
            ""eventTime"": ""2017-11-07T20:09:22.5674003Z"",
            ""id"": ""4c2359fe-001e-00ba-0e04-58586806d298"",
            ""data"": {
                ""validationCode"": ""512d38b6-c7b8-40c8-89fe-f46f9e9622b6""
            },
            ""dataVersion"": ""1"",
            ""metadataVersion"": ""1""
            }]";
        
        _mockHttpRequest = CreateMockRequest(eventJson);
        
        var result = await _handleDocumentDeletedEvent.RunAsync(_mockHttpRequest.Object);

        using (new AssertionScope())
        {
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var responseValue = objectResult.Value.ToString();
            responseValue.Should().Be("{ ValidationResponse = 512d38b6-c7b8-40c8-89fe-f46f9e9622b6 }");

            _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName),
                Times.Never);
        }
    }
    
    private static Mock<HttpRequest> CreateMockRequest(string body)
    {            
        var ms = new MemoryStream();
        var sw = new StreamWriter(ms);
 
        //var json = JsonConvert.SerializeObject(body, Formatting.None);
 
        sw.Write(body);
        sw.Flush();
 
        ms.Position = 0;
 
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(x => x.Body).Returns(ms);
 
        return mockRequest;
    }
}
