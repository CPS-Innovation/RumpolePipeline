using System;
using System.IO;

namespace pdf_generator.Services.PdfService
{
    public class PdfOrchestratorService : IPdfOrchestratorService
    {
        private readonly IPdfService _wordsPdfService;
        private readonly IPdfService _cellsPdfService;
        private readonly IPdfService _slidesPdfService;

        public PdfOrchestratorService(
            IPdfService wordsPdfService,
            IPdfService cellsPdfService,
            IPdfService slidesPdfService)
        {
            _wordsPdfService = wordsPdfService;
            _cellsPdfService = cellsPdfService;
            _slidesPdfService = slidesPdfService;
        }

        public Stream ReadToPdfStream(Stream inputStream, string fileType)
        {
            using var pdfStream = new MemoryStream(); //TODO need using?
            //TODO other file types
            switch (fileType)
            {
                case "docx":
                    _wordsPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "xls":
                    _cellsPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
                case "pptx":
                    _slidesPdfService.ReadToPdfStream(inputStream, pdfStream);
                    break;
            }

            return pdfStream;
        }
    }
}
