using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
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

namespace rumpolepipeline.tests.pdf_generator.Functions
{
	public class GeneratePdfTests
	{
        private readonly string _serializedGeneratePdfRequest;
		private readonly HttpRequestMessage _httpRequestMessage;
		private readonly GeneratePdfRequest _generatePdfRequest;
		private readonly string _blobName;
		private readonly Stream _documentStream;
		private readonly Stream _pdfStream;
		private readonly string _serializedGeneratePdfResponse;
		private HttpResponseMessage _errorHttpResponseMessage;
		private string _errorMessage;

		private readonly Mock<IAuthorizationHandler> _mockAuthorizationHandler;
		private readonly Mock<ClaimsPrincipal> _mockClaimsPrincipal;
		private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IDocumentExtractionService> _mockDocumentExtractionService;
		private readonly Mock<IBlobStorageService> _mockBlobStorageService;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;

		private readonly GeneratePdf _generatePdf;

		public GeneratePdfTests()
		{
            var fixture = new Fixture();
			_serializedGeneratePdfRequest = fixture.Create<string>();
			_httpRequestMessage = new HttpRequestMessage()
			{
				Content = new StringContent(_serializedGeneratePdfRequest)
			};
			_generatePdfRequest = fixture.Build<GeneratePdfRequest>()
									.With(r => r.FileName, "Test.doc")
									.Create();
			_blobName = $"{_generatePdfRequest.CaseId}/pdfs/{_generatePdfRequest.DocumentId}.pdf";
			_documentStream = new MemoryStream();
			_pdfStream = new MemoryStream();
			_serializedGeneratePdfResponse = fixture.Create<string>();
			_errorMessage = fixture.Create<string>();

			_mockAuthorizationHandler = new Mock<IAuthorizationHandler>();
			_mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockValidatorWrapper = new Mock<IValidatorWrapper<GeneratePdfRequest>>();
			_mockDocumentExtractionService = new Mock<IDocumentExtractionService>();
			_mockBlobStorageService = new Mock<IBlobStorageService>();
			var mockPdfOrchestratorService = new Mock<IPdfOrchestratorService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();

			_mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out _errorMessage))
				.Returns(true);
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
								_mockAuthorizationHandler.Object,
								_mockJsonConvertWrapper.Object,
								mockValidatorWrapper.Object,
								_mockDocumentExtractionService.Object,
								_mockBlobStorageService.Object,
								mockPdfOrchestratorService.Object,
								_mockExceptionHandler.Object);

            _errorHttpResponseMessage = new HttpResponseMessage();
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

			var response = await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
		{
			_generatePdfRequest.FileName = "Test.pdf";
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.DocumentId, _generatePdfRequest.FileName, It.IsAny<string>()))
				.ReturnsAsync(_documentStream);

			await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_documentStream, _blobName));
		}

		[Fact]
		public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
		{
			await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_pdfStream, _blobName));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsExpectedContent()
		{
			var response = await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

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

			var response = await _generatePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

