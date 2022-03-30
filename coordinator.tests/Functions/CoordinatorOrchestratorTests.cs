using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain;
using coordinator.Domain.CoreDataApi;
using coordinator.Domain.Tracker;
using coordinator.Functions;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorOrchestratorTests
    {
        private Fixture _fixture;
        private CoordinatorOrchestrationPayload _payload;
        private string _accessToken;
        private CaseDetails _caseDetails;
        private string _transactionId;
        private List<TrackerDocument> _trackerDocuments;

        private Mock<ILogger<CoordinatorOrchestrator>> _mockLogger;
        private Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private Mock<ITracker> _mockTracker;

        private CoordinatorOrchestrator CoordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Build<CoordinatorOrchestrationPayload>()
                        .With(p => p.ForceRefresh, false)
                        .Create();
            _accessToken = _fixture.Create<string>();
            _caseDetails = _fixture.Create<CaseDetails>();
            _transactionId = _fixture.Create<string>();
            _trackerDocuments = _fixture.Create<List<TrackerDocument>>();

            _mockLogger = new Mock<ILogger<CoordinatorOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.InstanceId)
                .Returns(_transactionId);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == _payload.CaseId.ToString())))
                .Returns(_mockTracker.Object);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), _payload.AccessToken))
                .ReturnsAsync(_accessToken);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDetails>(nameof(GetCaseDetailsById), It.Is<GetCaseDetailsByIdActivityPayload>(p => p.CaseId == _payload.CaseId && p.AccessToken == _accessToken)))
                .ReturnsAsync(_caseDetails);

            _mockTracker.Setup(tracker => tracker.GetDocuments()).ReturnsAsync(_trackerDocuments);

            CoordinatorOrchestrator = new CoordinatorOrchestrator(_mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(default(CoordinatorOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDocumentsWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);

            var documents = await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task Run_DoesNotInitialiseWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);

            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Never);
        }

        [Fact]
        public async Task Run_Tracker_InitialisesWheTrackerIsAlreadyProcessedAndForceRefreshIsTrue()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _payload.ForceRefresh = true;

            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId));
        }

        [Fact]
        public async Task Run_Tracker_Initialises()
        {
            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId));
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachDocumentId()
        {
            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in _caseDetails.Documents)
            {
                _mockDurableOrchestrationContext.Verify(
                    context => context.CallSubOrchestratorAsync(
                        nameof(CaseDocumentOrchestrator),
                        It.Is<CaseDocumentOrchestrationPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == document.Id)));
            }
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentIds()
        {
            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            var documentIds = _caseDetails.Documents.Select(d => d.Id);
            _mockTracker.Verify(tracker => tracker.RegisterDocumentIds(documentIds));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterCompleted());
        }

        [Fact]
        public async Task Run_ReturnsDocuments()
        {
            var documents = await CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), _payload.AccessToken))
                .ThrowsAsync(new Exception("Test Exception"));
            await Assert.ThrowsAsync<Exception>(() => CoordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }
    }
}
