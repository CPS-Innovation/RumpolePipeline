using System.ComponentModel.DataAnnotations;
using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Storage.Queues.Models;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.QueueItems;
using Common.Services.BlobStorageService.Contracts;
using Common.Wrappers;
using document_evaluator.Functions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace document_evaluation.tests.Functions;

public class UpdateBlobStorageTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IBlobStorageService> _mockBlobStorageService;
    private readonly Mock<ILogger<UpdateBlobStorage>> _mockLogger;
    private readonly UpdateBlobStorageQueueItem _updateMessage;

    private QueueMessage _queueMessage;
    private readonly UpdateBlobStorage _updateBlobStorage;

    public UpdateBlobStorageTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _updateMessage = _fixture.Create<UpdateBlobStorageQueueItem>();
        _mockLogger = new Mock<ILogger<UpdateBlobStorage>>();

        _queueMessage = QueuesModelFactory.QueueMessage(_fixture.Create<string>(), _fixture.Create<string>(), 
            _fixture.Create<UpdateBlobStorageQueueItem>().ToJson(), 1);
        
        var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
        var mockValidatorWrapper = new Mock<IValidatorWrapper<UpdateBlobStorageQueueItem>>();
        _mockBlobStorageService = new Mock<IBlobStorageService>();
        
        mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<UpdateBlobStorageQueueItem>(_queueMessage.MessageText))
            .Returns(_updateMessage);
        mockValidatorWrapper.Setup(wrapper => wrapper.Validate(It.IsAny<UpdateBlobStorageQueueItem>())).Returns(new List<ValidationResult>());
        
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.UpdateBlobStorageQueueName]).Returns($"update-blob-storage");

        _mockBlobStorageService.Setup(x => x.RemoveDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);

        _updateBlobStorage = new UpdateBlobStorage(mockJsonConvertWrapper.Object, mockValidatorWrapper.Object, mockConfiguration.Object, _mockBlobStorageService.Object);
    }
    
    [Theory]
    [InlineData("{}")]
    [InlineData("")]
    public async Task Run_ReturnsBadRequestWhenContentIsInvalid(string messageText)
    {
        _queueMessage = QueuesModelFactory.QueueMessage(_fixture.Create<string>(), _fixture.Create<string>(), 
            messageText, 1);
        
        await Assert.ThrowsAsync<Exception>(() => _updateBlobStorage.RunAsync(_queueMessage, _mockLogger.Object));
    }

    [Fact]
    public async Task Run_ValidMessageReceived_ProcessesMessage()
    {
        await _updateBlobStorage.RunAsync(_queueMessage, _mockLogger.Object);
        
        _mockBlobStorageService.Verify(x => x.RemoveDocumentAsync(_updateMessage.BlobName, _updateMessage.CorrelationId), Times.Once);
    }
}
