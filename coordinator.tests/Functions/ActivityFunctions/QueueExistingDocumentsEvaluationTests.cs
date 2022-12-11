using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Functions.ActivityFunctions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class QueueExistingDocumentsEvaluationTests
    {
        private readonly QueueExistingDocumentsEvaluationPayload _payload;
        private readonly string _content;
        private const string QueueName = "evaluate-existing-documents";

        private readonly Mock<IStorageQueueService> _mockStorageQueueService;
        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly QueueExistingDocumentsEvaluation _evaluateExistingDocuments;

        public QueueExistingDocumentsEvaluationTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<QueueExistingDocumentsEvaluationPayload>();
            _content = fixture.Create<string>();

            _mockStorageQueueService = new Mock<IStorageQueueService>();
            var mockJsonConverterWrapper = new Mock<IJsonConvertWrapper>();
            var mockConfiguration = new Mock<IConfiguration>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            mockJsonConverterWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<EvaluateExistingDocumentsRequest>(r => 
                    r.CaseId == _payload.CaseId && r.CorrelationId == _payload.CorrelationId))).Returns(_content);
            mockConfiguration.Setup(x => x[ConfigKeys.SharedKeys.EvaluateExistingDocumentsQueueName]).Returns(QueueName);
            _mockStorageQueueService.Setup(client => client.AddNewMessage(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var mockLogger = new Mock<ILogger<QueueExistingDocumentsEvaluation>>();
            _evaluateExistingDocuments = new QueueExistingDocumentsEvaluation(mockLogger.Object, mockJsonConverterWrapper.Object, mockConfiguration.Object, _mockStorageQueueService.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(default(QueueExistingDocumentsEvaluationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCaseIdIsZero_ThrowsArgumentException()
        {
            _payload.CaseId = 0;
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_WhenCaseUrnIsNullOrWhitespace_ThrowsArgumentException(string caseUrn)
        {
            _payload.CaseUrn = caseUrn;
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenCorrelationIdIsEmpty_ThrowsArgumentException()
        {
            _payload.CorrelationId = Guid.Empty;
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenDocumentsCollectionIsEmpty_ThrowsArgumentNullException()
        {
            _payload.CaseDocuments = null;
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentNullException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }
        
        [Fact]
        public async Task Run_WhenDocumentsCollectionIsZeroLength_ThrowsArgumentException()
        {
            _payload.CaseDocuments = new List<CaseDocument>();
            _mockDurableActivityContext.Setup(context => context.GetInput<QueueExistingDocumentsEvaluationPayload>())
                .Returns(_payload);

            await Assert.ThrowsAsync<ArgumentException>(() => _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_WhenAllIsWell_AddsTheMessageToTheQueue()
        {
            await _evaluateExistingDocuments.Run(_mockDurableActivityContext.Object);

            _mockStorageQueueService.Verify(x => x.AddNewMessage(_content, QueueName), Times.Exactly(1));
        }
    }
}
