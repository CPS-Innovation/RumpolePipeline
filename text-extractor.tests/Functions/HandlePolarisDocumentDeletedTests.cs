using System;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.EventGrid;
using Common.Constants;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions;

public class HandlePolarisDocumentDeletedTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IStorageQueueService> _mockStorageQueueService;

    private readonly HandlePolarisDocumentDeleted _handlePolarisDocumentDeleted;
    
    public HandlePolarisDocumentDeletedTests()
    {
        _fixture = new Fixture();
        _mockStorageQueueService = new Mock<IStorageQueueService>();

        var logger = new Mock<ILogger<HandlePolarisDocumentDeleted>>();
        var mockJsonConverterWrapper = new Mock<IJsonConvertWrapper>();
        _handlePolarisDocumentDeleted = new HandlePolarisDocumentDeleted(logger.Object, mockJsonConverterWrapper.Object, _mockStorageQueueService.Object);
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEvent_IsNull_ThrowArgumentNullException()
    {
        var act = async () =>
        {
           await _handlePolarisDocumentDeleted.RunAsync(null, new ExecutionContext());
        };

        using (new AssertionScope())
        {
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mockStorageQueueService.Verify(s => s.AddNewMessage(It.IsAny<string>(), ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName), 
                Times.Never);
        }
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsNotBlobDeleted_ThenTheEventIsIgnored()
    {
        var evt = _fixture.Create<EventGridEvent>();

        await _handlePolarisDocumentDeleted.RunAsync(evt, new ExecutionContext());
        
        _mockStorageQueueService.Verify(s => s.AddNewMessage(It.IsAny<string>(), ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName), 
            Times.Never);
    }
    
    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsBlobDeleted_ButNoEventDataReceived_ThrowsNullReferenceException()
    {
        var evt = _fixture.Create<EventGridEvent>();
        evt.EventType = EventGridEvents.BlobDeletedEvent;
        evt.Data = null;
        
        var act = async () =>
        {
            await _handlePolarisDocumentDeleted.RunAsync(evt, new ExecutionContext());
        };

        using (new AssertionScope())
        {
            await act.Should().ThrowAsync<NullReferenceException>();
            _mockStorageQueueService.Verify(s => s.AddNewMessage(It.IsAny<string>(), ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName), 
                Times.Never);
        }
    }
    
    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsBlobDeleted_AndEventDataIsReceivedAsExpected_ThenTheEventIsProcessed_UsingTheCorrectParams()
    {
        var evt = _fixture.Create<EventGridEvent>();
        evt.EventType = EventGridEvents.BlobDeletedEvent;
        
        const string eventJson = @"{
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
                    }";
        evt.Data = new BinaryData(eventJson);
        
        await _handlePolarisDocumentDeleted.RunAsync(evt, new ExecutionContext());
        
        _mockStorageQueueService.Verify(s => s.AddNewMessage(It.IsAny<string>(), ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName), 
            Times.Once);
    }
}
