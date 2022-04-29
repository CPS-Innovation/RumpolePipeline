using System;
using System.IO;
using pdf_generator.Domain;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class PdfOrchestratorService : IPdfOrchestratorService
    {
        private readonly IPdfService _wordsPdfService;
        private readonly IPdfService _cellsPdfService;
        private readonly IPdfService _slidesPdfService;
        private readonly IPdfService _imagingPdfService;
        private readonly IPdfService _diagramPdfService;
        private readonly IPdfService _htmlPdfService;
        private readonly IPdfService _emailPdfService;

        public PdfOrchestratorService(
            IPdfService wordsPdfService,
            IPdfService cellsPdfService,
            IPdfService slidesPdfService,
            IPdfService imagingPdfService,
            IPdfService diagramPdfService,
            IPdfService htmlPdfService,
            IPdfService emailPdfService)
        {
            _wordsPdfService = wordsPdfService;
            _cellsPdfService = cellsPdfService;
            _slidesPdfService = slidesPdfService;
            _imagingPdfService = imagingPdfService;
            _diagramPdfService = diagramPdfService;
            _htmlPdfService = htmlPdfService;
            _emailPdfService = emailPdfService;
        }

        public Stream ReadToPdfStream(Stream inputStream, FileType fileType, string documentId)
        {
            try
            {
                var pdfStream = new MemoryStream();
                switch (fileType)
                {
                    case FileType.DOC:
                    case FileType.DOCX:
                    case FileType.DOCM:
                    case FileType.RTF:
                    case FileType.TXT:
                        _wordsPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.XLS:
                    case FileType.XLSX:
                        _cellsPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.PPT:
                    case FileType.PPTX:
                        _slidesPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.BMP:
                    case FileType.GIF:
                    case FileType.JPG:
                    case FileType.JPEG:
                    case FileType.TIF:
                    case FileType.TIFF:
                    case FileType.PNG:
                        _imagingPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.VSD:
                        _diagramPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.HTML:
                        _htmlPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                    case FileType.MSG:
                        _emailPdfService.ReadToPdfStream(inputStream, pdfStream);
                        break;
                }

                return pdfStream;
            }
            catch(Exception exception)
            {
                throw new PdfConversionException(documentId, exception.Message);
            }
        }
    }
}
