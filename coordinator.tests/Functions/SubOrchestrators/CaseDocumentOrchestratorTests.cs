using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestratorTests
    {
        private readonly CaseDocumentOrchestrationPayload _payload;
        private readonly DurableHttpRequest _durableRequest;
        private readonly string _content;
        private readonly GeneratePdfResponse _pdfResponse;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ITracker> _mockTracker;

        private readonly CaseDocumentOrchestrator _caseDocumentOrchestrator;

        public CaseDocumentOrchestratorTests()
        {
            var fixture = new Fixture();
            _payload = fixture.Create<CaseDocumentOrchestrationPayload>();
            _durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("http://www.google.co.uk"));
            _content = fixture.Create<string>();
            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: _content);
            _pdfResponse = fixture.Create<GeneratePdfResponse>();

            var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            var mockLogger = new Mock<ILogger<CaseDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();

            mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfResponse>(_content)).Returns(_pdfResponse);

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateGeneratePdfHttpRequest),
                It.Is<CreateGeneratePdfHttpRequestActivityPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == _payload.DocumentId && p.FileName == _payload.FileName)))
                    .ReturnsAsync(_durableRequest);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<DurableHttpRequest>(
                nameof(CreateTextExtractorHttpRequest),
                It.Is<CreateTextExtractorHttpRequestActivityPayload>(p => p.CaseId == _payload.CaseId && p.DocumentId == _payload.DocumentId && p.BlobName == _pdfResponse.BlobName)))
                    .ReturnsAsync(_durableRequest);
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest)).ReturnsAsync(durableResponse);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == _payload.CaseId.ToString())))
                .Returns(_mockTracker.Object);

            _caseDocumentOrchestrator = new CaseDocumentOrchestrator(mockJsonConvertWrapper.Object, mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(default(CaseDocumentOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersPdfBlobName()
        {
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterPdfBlobName(It.Is<RegisterPdfBlobNameArg>(a => a.DocumentId == _payload.DocumentId && a.BlobName == _pdfResponse.BlobName)));
        }

        [Fact]
        public async Task Run_Tracker_RegistersIndexed()
        {
            await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterIndexed(_payload.DocumentId));
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            await Assert.ThrowsAsync<HttpRequestException>(() => _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentNotFoundInCDEWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotFound, content: _content));

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterDocumentNotFoundInCde(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedToConvertToPdfWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotImplemented, content: _content));

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnableToConvertDocumentToPdf(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_RegistersUnexpectedDocumentFailureWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnexpectedPdfDocumentFailure(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_RegistersUnexpectedDocumentFailureWhenUnhandledExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(_durableRequest))
                .ThrowsAsync(new Exception());

            try
            {
                await _caseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnexpectedPdfDocumentFailure(_payload.DocumentId));
            }
        }
    }
}
