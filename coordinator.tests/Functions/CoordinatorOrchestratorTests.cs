using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using Common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorOrchestratorTests
    {
        private readonly CoordinatorOrchestrationPayload _payload;
        private readonly string _accessToken;
        private readonly string _upstreamToken;
        private readonly CaseDocument[] _caseDocuments;
        private readonly string _transactionId;
        private readonly List<TrackerDocument> _trackerDocuments;
        private readonly DurableHttpRequest _durableRequest;
        private DurableHttpResponse _durableResponse;
        private readonly List<EvaluateDocumentResponse> _evaluateDocumentsResponse;
        
        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ITracker> _mockTracker;

        private readonly CoordinatorOrchestrator _coordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            var fixture = new Fixture();
            _accessToken = fixture.Create<string>();
            _upstreamToken = fixture.Create<string>();
            fixture.Create<Guid>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CoordinatorOrchestrationPayload>()
                        .With(p => p.ForceRefresh, false)
                        .With(p => p.AccessToken, _accessToken)
                        .With(p => p.UpstreamToken, _upstreamToken)
                        .Create();
            _caseDocuments = fixture.Create<CaseDocument[]>();

            _transactionId = fixture.Create<string>();
            _trackerDocuments = fixture.Create<List<TrackerDocument>>();
            _evaluateDocumentsResponse = fixture.CreateMany<EvaluateDocumentResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<CoordinatorOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();
            
            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]).Returns("300");
            
            _mockTracker.Setup(tracker => tracker.GetDocuments()).ReturnsAsync(_trackerDocuments);

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.InstanceId)
                .Returns(_transactionId);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == string.Concat(_payload.CaseUrn, "-", _payload.CaseId.ToString()))))
                .Returns(_mockTracker.Object);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<string>(nameof(GetOnBehalfOfAccessToken), _payload.AccessToken))
                .ReturnsAsync(_accessToken);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CaseId == _payload.CaseId 
                    && p.UpstreamToken == _payload.UpstreamToken && p.CorrelationId == _payload.CorrelationId)))
                .ReturnsAsync(_caseDocuments);
            
            _durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: _evaluateDocumentsResponse.ToJson());
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<DurableHttpRequest>(nameof(CreateEvaluateExistingDocumentsHttpRequest),
                It.IsAny<CreateEvaluateExistingDocumentsHttpRequestActivityPayload>())).ReturnsAsync(_durableRequest);
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest)).ReturnsAsync(_durableResponse);
            
            _coordinatorOrchestrator = new CoordinatorOrchestrator(mockConfiguration.Object, mockLogger.Object, new JsonConvertWrapper());
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(default(CoordinatorOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDocumentsWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);

            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task Run_DoesNotInitialiseWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Never);
        }

        [Fact]
        public async Task Run_Tracker_InitialisesWheTrackerIsAlreadyProcessedAndForceRefreshIsTrue()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _payload.ForceRefresh = true;

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId));
        }

        [Fact]
        public async Task Run_Tracker_Initialises()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId));
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentsNotFoundInCDEWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CaseId == _payload.CaseId && p.UpstreamToken == _upstreamToken)))
                .ReturnsAsync(new CaseDocument[] { });

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterNoDocumentsFoundInCDE());
        }


        [Fact]
        public async Task Run_ReturnsEmptyListOfDocumentsWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CaseId == _payload.CaseId && p.UpstreamToken == _upstreamToken)))
                .ReturnsAsync(new CaseDocument[] { });

            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEmpty();
        }


        [Fact]
        public async Task Run_Tracker_RegistersDocumentIds()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            var documentIds = _caseDocuments.Select(d => d.DocumentId);
            _mockTracker.Verify(tracker => tracker.RegisterDocumentIds(documentIds));
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachDocumentId()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in _caseDocuments)
            {
                _mockDurableOrchestrationContext.Verify(
                    context => context.CallSubOrchestratorAsync(
                        nameof(CaseDocumentOrchestrator),
                        It.Is<CaseDocumentOrchestrationPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == document.DocumentId)));
            }
        }

        [Fact]
        public async Task Run_DoesNotThrowWhenSubOrchestratorCallFails()
        {
            _mockDurableOrchestrationContext.Setup(
                context => context.CallSubOrchestratorAsync(nameof(CaseDocumentOrchestrator), It.IsAny<CaseDocumentOrchestrationPayload>()))
                    .ThrowsAsync(new Exception());
            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }
        
        [Fact]
        public async Task Run_RegistersUnexpectedDocumentEvaluationFailure_WhenCallToEvaluationFails()
        {
            _durableResponse = new DurableHttpResponse(HttpStatusCode.BadRequest, content: _evaluateDocumentsResponse.ToJson());
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest)).ReturnsAsync(_durableResponse);
            
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
            
            _mockTracker.Verify(tracker => tracker.RegisterUnexpectedExistingDocumentsEvaluationFailure());
        }

        [Fact]
        public async Task Run_ThrowsCoordinatorOrchestrationExceptionWhenAllDocumentsHaveFailed()
        {
            _mockTracker.Setup(t => t.AllDocumentsFailed()).ReturnsAsync(true);

            await Assert.ThrowsAsync<CoordinatorOrchestrationException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterCompleted());
        }

        [Fact]
        public async Task Run_ReturnsDocuments()
        {
            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }
        
        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CaseId == _payload.CaseId && p.UpstreamToken == _upstreamToken)))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CaseId == _payload.CaseId && p.UpstreamToken == _upstreamToken)))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterFailed());
            }
        }
    }
}
