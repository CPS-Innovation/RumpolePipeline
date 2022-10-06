using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using common.Domain.Exceptions;
using common.Handlers;
using common.Wrappers;
using FluentAssertions;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Domain.Requests;
using text_extractor.Functions;
using text_extractor.Handlers;
using text_extractor.Services.OcrService;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Functions
{
	public class ExtractTextTests
	{
        private readonly string _serializedExtractTextRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly ExtractTextRequest _extractTextRequest;
		private HttpResponseMessage _errorHttpResponseMessage;
		
		private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<ISearchIndexService> _mockSearchIndexService;
		private readonly Mock<IExceptionHandler> _mockExceptionHandler;
		private readonly Mock<AnalyzeResults> _mockAnalyzeResults;

		private readonly Mock<ILogger<ExtractText>> _mockLogger;
		private readonly Guid _correlationId;

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
			
			_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockValidatorWrapper = new Mock<IValidatorWrapper<ExtractTextRequest>>();
			var mockOcrService = new Mock<IOcrService>();
			_mockSearchIndexService = new Mock<ISearchIndexService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();
			_mockAnalyzeResults = new Mock<AnalyzeResults>();

			_correlationId = fixture.Create<Guid>();

			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(true, fixture.Create<string>()));
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Returns(_extractTextRequest);
			mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_extractTextRequest)).Returns(new List<ValidationResult>());
			mockOcrService.Setup(service => service.GetOcrResultsAsync(_extractTextRequest.BlobName, It.IsAny<Guid>()))
				.ReturnsAsync(_mockAnalyzeResults.Object);

			_mockLogger = new Mock<ILogger<ExtractText>>();

			_extractText = new ExtractText(
								_mockAuthorizationValidator.Object,
								_mockJsonConvertWrapper.Object,
								mockValidatorWrapper.Object,
								mockOcrService.Object,
								_mockSearchIndexService.Object,
								_mockExceptionHandler.Object,
								_mockLogger.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsExceptionWhenCorrelationIdIsMissing()
		{
			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<ILogger<ExtractText>>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			
			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
		{
			_mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<Guid>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("X-Correlation-ID", _correlationId.ToString());

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");
			_httpRequestMessage.Headers.Add("X-Correlation-ID", _correlationId.ToString());

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_StoresOcrResults()
		{
			_httpRequestMessage.Headers.Add("X-Correlation-ID", _correlationId.ToString());
			await _extractText.Run(_httpRequestMessage);

			_mockSearchIndexService.Verify(service => service.StoreResultsAsync(_mockAnalyzeResults.Object, _extractTextRequest.CaseId, _extractTextRequest.DocumentId, _correlationId));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			_httpRequestMessage.Headers.Add("X-Correlation-ID", _correlationId.ToString());
			var response = await _extractText.Run(_httpRequestMessage);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsResponseWhenExceptionOccurs()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var exception = new Exception();
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Throws(exception);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<Exception>(), It.IsAny<Guid>(), It.IsAny<string>(), _mockLogger.Object))
				.Returns(_errorHttpResponseMessage);

			var response = await _extractText.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

