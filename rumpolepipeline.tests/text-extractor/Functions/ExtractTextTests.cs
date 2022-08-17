using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using AutoFixture;
using common.Domain.Exceptions;
using common.Handlers;
using common.Wrappers;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Moq;
using text_extractor.Domain.Requests;
using text_extractor.Functions.ProcessDocument;
using text_extractor.Handlers;
using text_extractor.Services.OcrService;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Functions
{
	public class ExtractTextTests
	{
        private readonly string _serializedExtractTextRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly ExtractTextRequest _extractTextRequest;
		private HttpResponseMessage _errorHttpResponseMessage = new();
		private string _errorMessage;

		private readonly Mock<IAuthorizationHandler> _mockAuthorizationHandler;
		private readonly Mock<ClaimsPrincipal> _mockClaimsPrincipal;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
		private readonly Mock<IExceptionHandler> _mockExceptionHandler;
		private readonly Mock<AnalyzeResults> _mockAnalyzeResults;

		private readonly ExtractText _extractText;

		public ExtractTextTests()
		{
            var fixture = new Fixture();
			_serializedExtractTextRequest = fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedExtractTextRequest)
			};
			_extractTextRequest = fixture.Create<ExtractTextRequest>();
			_errorMessage = fixture.Create<string>();

			_mockAuthorizationHandler = new Mock<IAuthorizationHandler>();
			_mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockValidatorWrapper = new Mock<IValidatorWrapper<ExtractTextRequest>>();
			var mockOcrService = new Mock<IOcrService>();
			_mockSearchIndexService = new Mock<ISearchIndexService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();
			_mockAnalyzeResults = new Mock<AnalyzeResults>();

			_mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out _errorMessage))
				.Returns(true);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Returns(_extractTextRequest);
			mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_extractTextRequest)).Returns(new List<ValidationResult>());
			mockOcrService.Setup(service => service.GetOcrResultsAsync(_extractTextRequest.BlobName))
				.ReturnsAsync(_mockAnalyzeResults.Object);

			_extractText = new ExtractText(
								_mockAuthorizationHandler.Object,
								_mockJsonConvertWrapper.Object,
								mockValidatorWrapper.Object,
								mockOcrService.Object,
								_mockSearchIndexService.Object,
								_mockExceptionHandler.Object);
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
		{
			_mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out _errorMessage))
				.Returns(false);
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await _extractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await _extractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_StoresOcrResults()
		{
			await _extractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockSearchIndexService.Verify(service => service.StoreResultsAsync(_mockAnalyzeResults.Object, _extractTextRequest.CaseId, _extractTextRequest.DocumentId));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _extractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsResponseWhenExceptionOccurs()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var exception = new Exception();
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Throws(exception);
			_mockExceptionHandler.Setup(handler => handler.HandleException(exception))
				.Returns(_errorHttpResponseMessage);

			var response = await _extractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

