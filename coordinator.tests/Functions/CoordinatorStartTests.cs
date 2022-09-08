using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using common.Domain.Exceptions;
using common.Handlers;
using coordinator.Domain;
using coordinator.Functions;
using coordinator.Handlers;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorStartTests
    {
        private readonly Fixture _fixture;
        private readonly int _caseIdNum;
        private readonly string _caseId;
        private readonly string _accessToken;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpRequestHeaders _httpRequestHeaders;
        private readonly HttpResponseMessage _httpResponseMessage;

        private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<CoordinatorStart>> _mockLogger;

        private readonly CoordinatorStart _coordinatorStart;

        public CoordinatorStartTests()
        {
            _fixture = new Fixture();
            _caseIdNum = _fixture.Create<int>();
            _caseId = _caseIdNum.ToString();
            _accessToken = _fixture.Create<string>();
            _httpRequestMessage = new HttpRequestMessage();
            _httpRequestMessage.RequestUri = new Uri("https://www.test.co.uk");
            _httpRequestHeaders = _httpRequestMessage.Headers;
            _httpResponseMessage = new HttpResponseMessage();

            _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            _mockLogger = new Mock<ILogger<CoordinatorStart>>();
            var mockAuthorizationValidator = new Mock<IAuthorizationValidator>();

            _httpRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");

            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));

            _mockDurableOrchestrationClient.Setup(client => client.CreateCheckStatusResponse(_httpRequestMessage, _caseId, false))
                .Returns(_httpResponseMessage);

            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>()))
                .Returns(new HttpResponseMessage(HttpStatusCode.InternalServerError));

            mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(It.IsNotNull<AuthenticationHeaderValue>()))
                .ReturnsAsync(new Tuple<bool, string>(true, _accessToken));
            mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(null))
                .ReturnsAsync(new Tuple<bool, string>(false, string.Empty));

            _coordinatorStart = new CoordinatorStart(_mockExceptionHandler.Object, _mockLogger.Object, mockAuthorizationValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAuthorizationHeaderIsMissing()
        {
            _httpRequestHeaders.Clear();
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>()))
                 .Returns(new HttpResponseMessage(HttpStatusCode.Unauthorized));

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenInvalidCaseId()
        {
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
                .Returns(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, "invalid", _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenForceRefreshIsInvalid()
        {
            _httpRequestMessage.RequestUri = new Uri("https://www.test.co.uk?force=invalid");
            _mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
                .Returns(new HttpResponseMessage(HttpStatusCode.BadRequest));

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            _mockDurableOrchestrationClient.Setup(client => client.StartNewAsync(nameof(CoordinatorOrchestrator), _caseId, It.IsAny<CoordinatorOrchestrationPayload>()))
                .Throws(new Exception());

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_StartsOrchestratorWhenOrchestrationStatusIsNull()
        {
            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));
            await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(CoordinatorOrchestrator),
                    _caseId,
                    It.Is<CoordinatorOrchestrationPayload>(p => p.CaseId == _caseIdNum && p.ForceRefresh == false && p.AccessToken == _accessToken)));
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Completed)]
        [InlineData(OrchestrationRuntimeStatus.Terminated)]
        [InlineData(OrchestrationRuntimeStatus.Failed)]
        [InlineData(OrchestrationRuntimeStatus.Canceled)]
        public async Task Run_StartsOrchestratorWhenOrchestrationHasConcluded(OrchestrationRuntimeStatus runtimeStatus)
        {
            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId, false, false, true))
               .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = runtimeStatus });
            await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(CoordinatorOrchestrator),
                    _caseId,
                    It.Is<CoordinatorOrchestrationPayload>(p => p.CaseId == _caseIdNum && p.ForceRefresh == false && p.AccessToken == _accessToken)));
        }

        [Fact]
        public async Task Run_SetsForceRefreshWhenValid()
        {
            var forceRefresh = _fixture.Create<bool>();
            _httpRequestMessage.RequestUri = new Uri($"https://www.test.co.uk?force={forceRefresh}");
            await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(CoordinatorOrchestrator),
                    _caseId,
                    It.Is<CoordinatorOrchestrationPayload>(p => p.ForceRefresh == forceRefresh)));
        }

        [Fact]
        public async Task Run_LogsInstanceId()
        {
            await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            _mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) => value.ToString() == $"Started {nameof(CoordinatorOrchestrator)} with instance id '{_caseId}'."),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));
        }

        [Fact]
        public async Task Run_DoesNotStartOrchestratorWhenOrchestrationHasNotConcluded()
        {
            var notStartingRuntimeStatuses = Enum.GetValues(typeof(OrchestrationRuntimeStatus))
                .Cast<OrchestrationRuntimeStatus>()
                .Except(new[] {
                    OrchestrationRuntimeStatus.Completed,
                    OrchestrationRuntimeStatus.Terminated,
                    OrchestrationRuntimeStatus.Failed,
                    OrchestrationRuntimeStatus.Canceled
                });

            foreach (var runtimeStatus in notStartingRuntimeStatuses)
            {
                _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId, false, false, true))
                    .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = runtimeStatus });

                await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);
            }

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(CoordinatorOrchestrator),
                    _caseId,
                    It.Is<CoordinatorOrchestrationPayload>(p => p.CaseId == _caseIdNum && p.ForceRefresh == false && p.AccessToken == _accessToken)),
                Times.Never);
        }

        [Fact]
        public async Task Run_ReturnsExpectedHttpResponseMessage()
        {
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.Should().Be(_httpResponseMessage);
        }
    }
}
