using System;
using System.IO;
using AutoFixture;
using Moq;
using pdf_generator.Domain;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.PdfService
{
	public class PdfOrchestratorServiceTests
	{
		private Fixture _fixture;
		private Stream _inputStream;
		private string _documentId;

		private Mock<IPdfService> _mockWordsPdfService;
		private Mock<IPdfService> _mockCellsPdfService;
		private Mock<IPdfService> _mockSlidesPdfService;
		private Mock<IPdfService> _mockImagingPdfService;
		private Mock<IPdfService> _mockDiagramPdfService;
		private Mock<IPdfService> _mockHtmlPdfService;
		private Mock<IPdfService> _mockEmailPdfService;

		private IPdfOrchestratorService PdfOrchestratorService;

		public PdfOrchestratorServiceTests()
		{
			_fixture = new Fixture();
			_inputStream = new MemoryStream();
			_documentId = _fixture.Create<string>();

			_mockWordsPdfService = new Mock<IPdfService>();
			_mockCellsPdfService = new Mock<IPdfService>();
			_mockSlidesPdfService = new Mock<IPdfService>();
			_mockImagingPdfService = new Mock<IPdfService>();
			_mockDiagramPdfService = new Mock<IPdfService>();
			_mockHtmlPdfService = new Mock<IPdfService>();
			_mockEmailPdfService = new Mock<IPdfService>();

			PdfOrchestratorService = new PdfOrchestratorService(
										_mockWordsPdfService.Object,
										_mockCellsPdfService.Object,
										_mockSlidesPdfService.Object,
										_mockImagingPdfService.Object,
										_mockDiagramPdfService.Object,
										_mockHtmlPdfService.Object,
										_mockEmailPdfService.Object);
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDoc()
        {
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOC, _documentId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
        }

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDocx()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOCX, _documentId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsDocm()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.DOCM, _documentId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsRtf()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.RTF, _documentId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsWordsServiceWhenFileTypeIsTxt()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TXT, _documentId);

			_mockWordsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsCellsServiceWhenFileTypeIsXls()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.XLS, _documentId);

			_mockCellsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsCellsServiceWhenFileTypeIsXlsx()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.XLSX, _documentId);

			_mockCellsPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsSlidesServiceWhenFileTypeIsPpt()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PPT, _documentId);

			_mockSlidesPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsSlidesServiceWhenFileTypeIsPptx()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PPTX, _documentId);

			_mockSlidesPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsBmp()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.BMP, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsGif()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.GIF, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsJpg()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.JPG, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsJpeg()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.JPEG, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsTif()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TIF, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsTiff()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.TIFF, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsImagingServiceWhenFileTypeIsPng()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.PNG, _documentId);

			_mockImagingPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsDiagramServiceWhenFileTypeIsVsd()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.VSD, _documentId);

			_mockDiagramPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsHtmlServiceWhenFileTypeIsHtml()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.HTML, _documentId);

			_mockHtmlPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_CallsEmailServiceWhenFileTypeIsMsg()
		{
			PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.MSG, _documentId);

			_mockEmailPdfService.Verify(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()));
		}

		[Fact]
		public void ReadToPdfStream_ThrowsFailedToConvertToPdfExceptionWhenExceptionOccurs()
		{
			_mockEmailPdfService.Setup(service => service.ReadToPdfStream(_inputStream, It.IsAny<MemoryStream>()))
				.Throws(new Exception());

			Assert.Throws<PdfConversionException>(() => PdfOrchestratorService.ReadToPdfStream(_inputStream, FileType.MSG, _documentId));
		}
	}
}

