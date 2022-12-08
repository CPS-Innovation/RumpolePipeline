using System;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.Requests;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions;

public class QueueUpdateSearchIndexByVersionTests
{
    private readonly QueueUpdateSearchIndexByVersionPayload _payload;
    private readonly string _content;

    private readonly Mock<IStorageQueueService> _mockStorageQueueService;
    private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

    private readonly QueueUpdateSearchIndexByVersion _updateSearchIndex;

    public QueueUpdateSearchIndexByVersionTests()
    {
        var fixture = new Fixture();
        _payload = fixture.Create<QueueUpdateSearchIndexByVersionPayload>();
        _content = fixture.Create<string>();

        _mockStorageQueueService = new Mock<IStorageQueueService>();
        var mockJsonConverterWrapper = new Mock<IJsonConvertWrapper>();
        _mockDurableActivityContext = new Mock<IDurableActivityContext>();

        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(_payload);

        mockJsonConverterWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<UpdateSearchIndexByVersionRequest>(r => r.CaseId == _payload.CaseId && r.CorrelationId == _payload.CorrelationId)))
            .Returns(_content);
        _mockStorageQueueService.Setup(client => client.AddNewMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var mockLogger = new Mock<ILogger<QueueUpdateSearchIndexByVersion>>();
        _updateSearchIndex = new QueueUpdateSearchIndexByVersion(mockLogger.Object, mockJsonConverterWrapper.Object, _mockStorageQueueService.Object);
    }

    [Fact]
    public async Task Run_ThrowsWhenPayloadIsNull()
    {
        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(default(QueueUpdateSearchIndexByVersionPayload));

        await Assert.ThrowsAsync<ArgumentException>(() => _updateSearchIndex.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
    {
        _payload.CaseId = 0;
        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _updateSearchIndex.Run(_mockDurableActivityContext.Object));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Run_WhenCaseUrnIsNullOrWhitespace_ThrowsArgumentException(string caseUrn)
    {
        _payload.CaseUrn = caseUrn;
        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _updateSearchIndex.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
    {
        _payload.CorrelationId = Guid.Empty;
        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _updateSearchIndex.Run(_mockDurableActivityContext.Object));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Run_WhenDocumentsIdIsEmpty_ThrowsArgumentNullException(string documentId)
    {
        _payload.DocumentId = documentId;
        _mockDurableActivityContext.Setup(context => context.GetInput<QueueUpdateSearchIndexByVersionPayload>())
            .Returns(_payload);

        await Assert.ThrowsAsync<ArgumentException>(() => _updateSearchIndex.Run(_mockDurableActivityContext.Object));
    }
    
    [Fact]
    public async Task Run_WhenAllIsWell_AddsTheMessageToTheQueue()
    {
        await _updateSearchIndex.Run(_mockDurableActivityContext.Object);

        _mockStorageQueueService.Verify(x => x.AddNewMessage(_content, It.IsAny<string>()), Times.Exactly(1));
    }
}