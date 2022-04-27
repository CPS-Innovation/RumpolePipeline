using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using AutoFixture;
using common.Wrappers;
using Moq;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Functions;
using pdf_generator.Handlers;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentExtractionService;
using pdf_generator.Services.PdfService;
using pdf_generator.Wrappers;

namespace pdf_generator.tests.Functions
{
	public class GeneratePdfTests
	{
		private Fixture _fixture;
		private HttpRequestMessage _httpRequestMessage;
		private GeneratePdfRequest _generatePdfRequest;
		private ICollection<ValidationResult> _validationResults;
		private Stream _pdfStream;

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
			_httpRequestMessage = new HttpRequestMessage();
			_generatePdfRequest = _fixture.Create<GeneratePdfRequest>();
			_validationResults = new List<ValidationResult>();
			_pdfStream = new MemoryStream();

			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockValidatorWrapper = new Mock<IValidatorWrapper<GeneratePdfRequest>>();
			_mockDocumentExtractionService = new Mock<IDocumentExtractionService>();
			_mockBlobStorageService = new Mock<IBlobStorageService>();
			_mockPdfOrchestratorService = new Mock<IPdfOrchestratorService>();
			_mockExceptionHandler = new Mock<IExceptionHandler>();

			//TODO setups and tests

			GeneratePdf = new GeneratePdf(
								_mockJsonConvertWrapper.Object,
								_mockValidatorWrapper.Object,
								_mockDocumentExtractionService.Object,
								_mockBlobStorageService.Object,
								_mockPdfOrchestratorService.Object,
								_mockExceptionHandler.Object);
		}
	}
}

