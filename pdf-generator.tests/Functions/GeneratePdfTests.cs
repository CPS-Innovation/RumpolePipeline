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
		private Fixture _fixture;
		private string _serializedGeneratePdfRequest;
		private HttpRequestMessage _httpRequestMessage;
		private GeneratePdfRequest _generatePdfRequest;
		private string _blobName;
		private Stream _documentStream;
		private Stream _pdfStream;
		private string _serializedGeneratePdfResponse;
		private HttpResponseMessage _errorHttpResponseMessage;
		private string _errorMessage;

		private Mock<IAuthorizationHandler> _mockAuthorizationHandler;
		private Mock<ClaimsPrincipal> _mockClaimsPrincipal;
		private Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
		private Mock<IValidatorWrapper<GeneratePdfRequest>> _mockValidatorWrapper;
		private Mock<IDocumentExtractionService> _mockDocumentExtractionService;
		private Mock<IBlobStorageService> _mockBlobStorageService;
		private Mock<IPdfOrchestratorService> _mockPdfOrchestratorService;
		private Mock<IExceptionHandler> _mockExceptionHandler;

		private GeneratePdf GeneratePdf;

		public GeneratePdfTests()
		{
			_fixture = new Fixture();
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
			_errorMessage = _fixture.Create<string>();

			_mockAuthorizationHandler = new Mock<IAuthorizationHandler>();
			_mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockValidatorWrapper = new Mock<IValidatorWrapper<GeneratePdfRequest>>();
			_mockDocumentExtractionService = new Mock<IDocumentExtractionService>();
			_mockBlobStorageService = new Mock<IBlobStorageService>();
			_mockPdfOrchestratorService = new Mock<IPdfOrchestratorService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();

			_mockAuthorizationHandler.Setup(handler => handler.IsAuthorized(_httpRequestMessage.Headers, _mockClaimsPrincipal.Object, out _errorMessage))
				.Returns(true);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<GeneratePdfRequest>(_serializedGeneratePdfRequest))
				.Returns(_generatePdfRequest);
			_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfResponse>(r => r.BlobName == _blobName)))
				.Returns(_serializedGeneratePdfResponse);
			_mockValidatorWrapper.Setup(wrapper => wrapper.Validate(_generatePdfRequest)).Returns(new List<ValidationResult>());
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.DocumentId, _generatePdfRequest.FileName, It.IsAny<string>()))
				.ReturnsAsync(_documentStream);
			_mockPdfOrchestratorService.Setup(service => service.ReadToPdfStream(_documentStream, FileType.DOC, _generatePdfRequest.DocumentId))
				.Returns(_pdfStream);

			GeneratePdf = new GeneratePdf(
								_mockAuthorizationHandler.Object,
								_mockJsonConvertWrapper.Object,
								_mockValidatorWrapper.Object,
								_mockDocumentExtractionService.Object,
								_mockBlobStorageService.Object,
								_mockPdfOrchestratorService.Object,
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

			var response = await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
			_errorHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
			_mockExceptionHandler.Setup(handler => handler.HandleException(It.IsAny<BadRequestException>()))
				.Returns(_errorHttpResponseMessage);
			_httpRequestMessage.Content = new StringContent(" ");

			var response = await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}

		[Fact]
		public async Task Run_UploadsDocumentStreamWhenFileTypeIsPdf()
		{
			_generatePdfRequest.FileName = "Test.pdf";
			_mockDocumentExtractionService.Setup(service => service.GetDocumentAsync(_generatePdfRequest.DocumentId, _generatePdfRequest.FileName, It.IsAny<string>()))
				.ReturnsAsync(_documentStream);

			await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_documentStream, _blobName));
		}

		[Fact]
		public async Task Run_UploadsPdfStreamWhenFileTypeIsNotPdf()
		{
			await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			_mockBlobStorageService.Verify(service => service.UploadDocumentAsync(_pdfStream, _blobName));
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.StatusCode.Should().Be(HttpStatusCode.OK);
		}

		[Fact]
		public async Task Run_ReturnsExpectedContent()
		{
			var response = await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

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

			var response = await GeneratePdf.Run(_httpRequestMessage, _mockClaimsPrincipal.Object);

			response.Should().Be(_errorHttpResponseMessage);
		}
	}
}

