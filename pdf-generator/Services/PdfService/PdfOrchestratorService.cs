using System.IO;

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

        public Stream ReadToPdfStream(Stream inputStream, string fileType)
        {
            var pdfStream = new MemoryStream();
            //TODO test all file types
            switch (fileType)
            {
                case "doc":
                case "docx":
                case "docm":
                case "rtf":
                case "txt":
                    _wordsPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "xls":
                case "xlsx":
                    _cellsPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "ppt":
                case "pptx":
                    _slidesPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "bmp":
                case "gif":
                case "jpg":
                case "jpeg":
                case "tif":
                case "tiff":
                case "png":
                    _imagingPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "vsd":
                    _diagramPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "html":
                    _htmlPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "msg":
                    _emailPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
            }

            //TODO throw exception if type not supported

            return pdfStream;
        }
    }
}
