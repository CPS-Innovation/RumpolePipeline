using System;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Common.Constants;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Functions;

public class HandlePolarisDocumentDeletedTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ISearchIndexService> _searchIndexService;

    private readonly HandlePolarisDocumentDeleted _handlePolarisDocumentDeleted;
    
    public HandlePolarisDocumentDeletedTests()
    {
        _fixture = new Fixture();
        _searchIndexService = new Mock<ISearchIndexService>();

        var logger = new Mock<ILogger<HandlePolarisDocumentDeleted>>();
        _handlePolarisDocumentDeleted = new HandlePolarisDocumentDeleted(logger.Object, _searchIndexService.Object);
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
            _searchIndexService.Verify(s => s.RemoveResultsForDocumentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()), 
                Times.Never);
        }
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsNotBlobDeleted_ThenTheEventIsIgnored()
    {
        var evt = _fixture.Create<EventGridEvent>();

        await _handlePolarisDocumentDeleted.RunAsync(evt, new ExecutionContext());
        
        _searchIndexService.Verify(s => s.RemoveResultsForDocumentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()),
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
            _searchIndexService.Verify(s => s.RemoveResultsForDocumentAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()), 
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
        
        var eventData = evt.Data.ToObjectFromJson<StorageBlobDeletedEventData>();
        var caseId = -1;
        var documentId = "";
        if (eventData != null)
        {
            var blobDetails = new Uri(eventData.Url).PathAndQuery.Split("/");
            caseId = int.Parse(blobDetails[2]);
            documentId = blobDetails[4].Replace(".pdf", "", StringComparison.OrdinalIgnoreCase);
        }

        await _handlePolarisDocumentDeleted.RunAsync(evt, new ExecutionContext());
        
        _searchIndexService.Verify(s => s.RemoveResultsForDocumentAsync(caseId, documentId, It.IsAny<Guid>()), Times.Once);
    }
}
