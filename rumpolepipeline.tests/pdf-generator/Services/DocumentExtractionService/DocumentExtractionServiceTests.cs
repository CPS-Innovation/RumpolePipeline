using System.Net;
using AutoFixture;
using FluentAssertions;
using Moq;
using Moq.Protected;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories;
using pdf_generator.Services.DocumentExtractionService;
using Xunit;

namespace rumpolepipeline.tests.pdf_generator.Services.DocumentExtractionService
{
	public class DocumentExtractionServiceTests
	{
        private readonly string _documentId;
        private readonly string _fileName;
        private readonly string _accessToken;
        private readonly HttpResponseMessage _httpResponseMessage;

        private readonly IDocumentExtractionService _documentExtractionService;

        public DocumentExtractionServiceTests()
        {
            var fixture = new Fixture();
            _documentId = fixture.Create<string>();
            _fileName = fixture.Create<string>();
            _accessToken = fixture.Create<string>();
            var httpRequestMessage = new HttpRequestMessage();
            Stream documentStream = new MemoryStream();
            _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(documentStream)
            };
            

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_httpResponseMessage);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            var mockHttpRequestFactory = new Mock<IDocumentExtractionHttpRequestFactory>();

            mockHttpRequestFactory.Setup(factory => factory.Create($"doc-fetch/{_documentId}/{_fileName}", _accessToken))
                .Returns(httpRequestMessage);

            _documentExtractionService = new global::pdf_generator.Services.DocumentExtractionService.DocumentExtractionService(httpClient, mockHttpRequestFactory.Object);
        }

        [Fact]
        public async Task GetDocumentAsync_ReturnsExpectedStream()
        {
            var documentStream = await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);

            documentStream.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
        {
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

            await Assert.ThrowsAsync<HttpException>(() => _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken));
        }

        [Fact]
        public async Task GetDocumentAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.NotFound;
            _httpResponseMessage.StatusCode = expectedStatusCode;

            try
            {
                await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);
            }
            catch (HttpException exception)
            {
                exception.StatusCode.Should().Be(expectedStatusCode);
            }
        }

        [Fact]
        public async Task GetDocumentAsync_HttpExceptionHasHttpRequestExceptionAsInnerExceptionWhenResponseStatusCodeIsNotSuccess()
        {
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;
            _httpResponseMessage.Content = new StringContent(string.Empty);

            try
            {
                await _documentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);
            }
            catch (HttpException exception)
            {
                exception.InnerException.Should().BeOfType<HttpRequestException>();
            }
        }
    }
}

