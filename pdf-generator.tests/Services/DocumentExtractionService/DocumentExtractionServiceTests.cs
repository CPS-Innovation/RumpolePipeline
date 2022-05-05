using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using Moq.Protected;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Factories;
using pdf_generator.Services.DocumentExtractionService;
using Xunit;

namespace pdf_generator.tests.Services.DocumentExtractionService
{
	public class DocumentExtractionServiceTests
	{
        private Fixture _fixture;
        private string _documentId;
        private string _fileName;
        private string _accessToken;
        private HttpRequestMessage _httpRequestMessage;
        private HttpResponseMessage _httpResponseMessage;
        private Stream _documentStream;

        private HttpClient _httpClient;
        private Mock<IDocumentExtractionHttpRequestFactory> _mockHttpRequestFactory;

        private IDocumentExtractionService DocumentExtractionService;

        public DocumentExtractionServiceTests()
        {
            _fixture = new Fixture();
            _documentId = _fixture.Create<string>();
            _fileName = _fixture.Create<string>();
            _accessToken = _fixture.Create<string>();
            _httpRequestMessage = new HttpRequestMessage();
            _documentStream = new MemoryStream();
            _httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(_documentStream)
            };
            

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", _httpRequestMessage, ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(_httpResponseMessage);
            _httpClient = new HttpClient(mockHttpMessageHandler.Object) { BaseAddress = new Uri("https://testUrl") };

            _mockHttpRequestFactory = new Mock<IDocumentExtractionHttpRequestFactory>();

            _mockHttpRequestFactory.Setup(factory => factory.Create($"doc-fetch/{_documentId}/{_fileName}", _accessToken))
                .Returns(_httpRequestMessage);

            DocumentExtractionService = new pdf_generator.Services.DocumentExtractionService.DocumentExtractionService(_httpClient, _mockHttpRequestFactory.Object);
        }

        [Fact]
        public async Task GetDocumentAsync_ReturnsExpectedStream()
        {
            var documentStream = await DocumentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);

            documentStream.Should().NotBeNull();
        }

        [Fact]
        public async Task GetDocumentAsync_ThrowsHttpExceptionWhenResponseStatusCodeIsNotSuccess()
        {
            _httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

            await Assert.ThrowsAsync<HttpException>(() => DocumentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken));
        }

        [Fact]
        public async Task GetDocumentAsync_HttpExceptionHasExpectedStatusCodeWhenResponseStatusCodeIsNotSuccess()
        {
            var expectedStatusCode = HttpStatusCode.NotFound;
            _httpResponseMessage.StatusCode = expectedStatusCode;

            try
            {
                await DocumentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);
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
                await DocumentExtractionService.GetDocumentAsync(_documentId, _fileName, _accessToken);
            }
            catch (HttpException exception)
            {
                exception.InnerException.Should().BeOfType<HttpRequestException>();
            }
        }
    }
}

