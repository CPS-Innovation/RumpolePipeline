using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using common.Wrappers;
using coordinator.Domain;
using coordinator.Domain.Requests;
using coordinator.Domain.Responses;
using coordinator.Domain.Tracker;
using coordinator.Functions.SubOrchestrators;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.SubOrchestrators
{
    public class CaseDocumentOrchestratorTests
    {
        private Fixture _fixture;
        private FunctionEndpointOptions _functionEndpoints;
        private CaseDocumentOrchestrationPayload _payload;
        private string _serializedRequest;
        private DurableHttpResponse _durableResponse;
        private string _content;
        private GeneratePdfResponse _pdfResponse;

        private Mock<IOptions<FunctionEndpointOptions>> _mockOptions;
        private Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private Mock<ILogger<CaseDocumentOrchestrator>> _mockLogger;
        private Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private Mock<ITracker> _mockTracker;

        private CaseDocumentOrchestrator CaseDocumentOrchestrator;

        public CaseDocumentOrchestratorTests()
        {
            _fixture = new Fixture();
            _functionEndpoints =
                _fixture.Build<FunctionEndpointOptions>()
                .With(o => o.GeneratePdf, "https://www.test.co.uk")
                .Create();
            _payload = _fixture.Create<CaseDocumentOrchestrationPayload>();
            _serializedRequest = _fixture.Create<string>();
            _content = _fixture.Create<string>();
            _durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: _content);
            _pdfResponse = _fixture.Create<GeneratePdfResponse>();

            _mockOptions = new Mock<IOptions<FunctionEndpointOptions>>();
            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockLogger = new Mock<ILogger<CaseDocumentOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();

            _mockOptions.Setup(options => options.Value).Returns(_functionEndpoints);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequest>(r => r.CaseId == _payload.CaseId && r.DocumentId == _payload.DocumentId)))
                .Returns(_serializedRequest);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfResponse>(_content)).Returns(_pdfResponse);

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(It.Is<DurableHttpRequest>(r => r.Method == HttpMethod.Post && r.Uri.OriginalString == _functionEndpoints.GeneratePdf && r.Content == _serializedRequest)))
                .ReturnsAsync(_durableResponse);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == _payload.CaseId.ToString())))
                .Returns(_mockTracker.Object);

            CaseDocumentOrchestrator = new CaseDocumentOrchestrator(_mockOptions.Object, _mockJsonConvertWrapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseDocumentOrchestrationPayload>()).Returns(default(CaseDocumentOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersPdfBlobName()
        {
            await CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterPdfBlobName(It.Is<RegisterPdfBlobNameArg>(a => a.DocumentId == _payload.DocumentId && a.BlobName == _pdfResponse.BlobName)));
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenCallToGeneratePdfReturnsNonOkResponse()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(It.Is<DurableHttpRequest>(r => r.Method == HttpMethod.Post && r.Uri.OriginalString == _functionEndpoints.GeneratePdf && r.Content == _serializedRequest)))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            await Assert.ThrowsAsync<HttpRequestException>(() => CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentNotFoundInCDEWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(It.Is<DurableHttpRequest>(r => r.Method == HttpMethod.Post && r.Uri.OriginalString == _functionEndpoints.GeneratePdf && r.Content == _serializedRequest)))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotFound, content: _content));

            try
            {
                await CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterDocumentNotFoundInCDE(_payload.DocumentId));
            }
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedToConvertToPdfWhenNotFoundStatusCodeReturned()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(It.Is<DurableHttpRequest>(r => r.Method == HttpMethod.Post && r.Uri.OriginalString == _functionEndpoints.GeneratePdf && r.Content == _serializedRequest)))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.NotImplemented, content: _content));

            try
            {
                await CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
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
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(It.Is<DurableHttpRequest>(r => r.Method == HttpMethod.Post && r.Uri.OriginalString == _functionEndpoints.GeneratePdf && r.Content == _serializedRequest)))
                .ReturnsAsync(new DurableHttpResponse(HttpStatusCode.InternalServerError, content: _content));

            try
            {
                await CaseDocumentOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterUnexpectedDocumentFailure(_payload.DocumentId));
            }
        }
    }
}
