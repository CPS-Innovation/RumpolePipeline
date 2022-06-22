using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
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

namespace pdf_generator.tests.Functions
{
	public class ExtractTextTests
	{
		private Fixture _fixture;
		private string _serializedExtractTextRequest;
		private HttpRequestMessage _httpRequestMessage;
		private ExtractTextRequest _extractTextRequest;
		private HttpResponseMessage _errorHttpResponseMessage;
		private string _errorMessage;

		private Mock<IAuthorizationHandler> _mockAuthorizationHandler;
		private Mock<ClaimsPrincipal> _mockClaimsPrincipal;
		private Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
		private Mock<IValidatorWrapper<ExtractTextRequest>> _mockValidatorWrapper;
		private Mock<IOcrService> _mockOcrService;
		private Mock<ISearchIndexService> _mockSearchIndexService;
		private Mock<IExceptionHandler> _mockExceptionHandler;
		private Mock<AnalyzeResults> _mockAnalyzeResults;

		private ExtractText ExtractText;

		public ExtractTextTests()
		{
			_fixture = new Fixture();
			_serializedExtractTextRequest = _fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedExtractTextRequest)
			};
			_extractTextRequest = _fixture.Create<ExtractTextRequest>();
			_errorMessage = _fixture.Create<string>();

			_mockAuthorizationHandler = new Mock<IAuthorizationHandler>();
			_mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockValidatorWrapper = new Mock<IValidatorWrapper<ExtractTextRequest>>();
			_mockOcrService = new Mock<IOcrService>();
			_mockSearchIndexService = new Mock<ISearchIndexService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();
			_mockAnalyzeResults = new Mock<AnalyzeResults>();

			_mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out _errorMessage))
				.Returns(true);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ExtractTextRequest>(_serializedExtractTextRequest))
				.Returns(_extractTextRequest);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_extractTextRequest)).Returns(new List<ValidationResult>());
			_mockOcrService.Setup(service => service.GetOcrResultsAsync(_extractTextRequest.BlobName))
				.ReturnsAsync(_mockAnalyzeResults.Object);

			ExtractText = new ExtractText(
								_mockAuthorizationHandler.Object,
								_mockJsonConvertWrapper.Object,
								_mockValidatorWrapper.Object,
								_mockOcrService.Object,
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

			var response = await ExtractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await ExtractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_StoresOcrResults()
		{
			await ExtractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockSearchIndexService.Verify(service => service.StoreResultsAsync(_mockAnalyzeResults.Object, _extractTextRequest.CaseId, _extractTextRequest.DocumentId));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await ExtractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

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

			var response = await ExtractText.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

