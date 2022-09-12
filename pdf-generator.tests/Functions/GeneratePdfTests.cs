using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using common.Domain.Exceptions;
using common.Handlers;
using common.Wrappers;
using FluentAssertions;
using Moq;
using pdf_generator.Domain;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Functions;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Functions
{
	public class GeneratePdfTests
	{
		private readonly Fixture _fixture = new();
        private readonly string _serializedGeneratePdfRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly GeneratePdfRequest _generatePdfRequest;
		private readonly string _blobName;
		private readonly Stream _documentStream;
		private readonly Stream _pdfStream;
		private readonly string _serializedGeneratePdfResponse;
		private HttpResponseMessage _errorHttpResponseMessage;
		
		private readonly Mock<IAuthorizationValidator> _mockAuthorizationValidator;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IDocumentExtractionService> _mockDocumentExtractionService;
		private readonly Mock<IBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;

		private readonly GeneratePdf _generatePdf;

		public GeneratePdfTests()
		{
            _serializedGeneratePdfRequest = _fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedGeneratePdfRequest)
			};
			_generatePdfRequest = _fixture.Build<GeneratePdfRequest>()
									.With(r => r.FileName, "Test.doc")
									.Create();
			_blobName = $"{_generatePdfRequest.CaseId}/pdfs/{_generatePdfRequest.DocumentId}.pdf";
			_documentStream = new MemoryStream();
			_pdfStream = new MemoryStream();
			_serializedGeneratePdfResponse = _fixture.Create<string>();
			
			_mockAuthorizationValidator = new Mock<IAuthorizationValidator>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockValidatorWrapper = new Mock<IValidatorWrapper<GeneratePdfRequest>>();
			_mockDocumentExtractionService = new Mock<IDocumentExtractionService>();
			_mockBlobStorageService = new Mock<IBlobStorageService>();
			var mockPdfOrchestratorService = new Mock<IPdfOrchestratorService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();

			_mockAuthorizationValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(), It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(true, _fixture.Create<string>()));
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfRequest>(_serializedGeneratePdfRequest))
				.Returns(_generatePdfRequest);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfResponse>(r => r.BlobName == _blobName)))
				.Returns(_serializedGeneratePdfResponse);
			mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(new List<ValidationResult>());
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.DocumentId, _generatePdfRequest.FileName, It.IsAny<string>()))
				.ReturnsAsync(_documentStream);
			mockPdfOrchestratorService.Setup(service => service.ReadToPdfStream(_documentStream, FileType.DOC, _generatePdfRequest.DocumentId))
				.Returns(_pdfStream);

			_generatePdf = new GeneratePdf(
								_mockAuthorizationValidator.Object,
								_mockJsonConvertWrapper.Object,
								mockValidatorWrapper.Object,
								_mockDocumentExtractionService.Object,
								_mockBlobStorageService.Object,
								mockPdfOrchestratorService.Object,
								_mockExceptionHandler.Object);
		}

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenUnauthorized()
		{
            _mockAuthorizationValidator.Setup(handler => handler.ValidateTokenAsync(It.IsAny<AuthenticationHeaderValue>(),It.IsAny<string>()))
				.ReturnsAsync(new Tuple<bool, string>(false, string.Empty));
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<UnauthorizedException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
		{
			_generatePdfRequest.FileName = "Test.pdf";
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.DocumentId, _generatePdfRequest.FileName, It.IsAny<string>()))
				.ReturnsAsync(_documentStream);

			await _generatePdf.Run(_httpRequestMessage);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_documentStream, _blobName));
		}

		[Fact]
		public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
		{
			await _generatePdf.Run(_httpRequestMessage);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_pdfStream, _blobName));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _generatePdf.Run(_httpRequestMessage);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsExpectedContent()
		{
			var response = await _generatePdf.Run(_httpRequestMessage);

			var content = await response.Content.ReadAsStringAsync();
			content.Should().Be(_serializedGeneratePdfResponse);
		}

		[Fact]
		public async Task Run_ReturnsResponseWhenExceptionOccurs()
		{
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			var exception = new Exception();
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfRequest>(_serializedGeneratePdfRequest))
				.Throws(exception);
			_mockExceptionHandler.Setup(handler => handler.HandleException(exception))
				.Returns(_errorHttpResponseMessage);

			var response = await _generatePdf.Run(_httpRequestMessage);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

