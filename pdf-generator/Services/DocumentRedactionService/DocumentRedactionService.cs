using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Services.BlobStorageService;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IBlobStorageService _blobStorageService;

        public DocumentRedactionService(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken)
        {
            var saveResult = new RedactPdfResponse();

            var fileName = redactPdfRequest.FileName;
            var document = await _blobStorageService.GetDocumentAsync(fileName);
            if (document == null)
            {
                saveResult.Succeeded = false;
                saveResult.Message = $"Invalid document - a document with filename '{fileName}' could not be retrieved for redaction purposes";
                return saveResult;
            }

            var fileNameWithoutExtension = fileName.IndexOf(".pdf", StringComparison.OrdinalIgnoreCase) > -1 ? fileName.Split(".pdf", StringSplitOptions.RemoveEmptyEntries)[0] : fileName;
            var newFileName = $"{fileNameWithoutExtension}_{DateTime.Now.Ticks.GetHashCode().ToString("x").ToUpper()}.pdf";

            using var doc = new Document(document);
            
            foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
            {
                var currentPage = redactionPage.PageIndex;
                var annotationPage = doc.Pages[currentPage];
                annotationPage.SetPageSize(redactionPage.Width, redactionPage.Height);
                foreach (var boxToDraw in redactionPage.RedactionCoordinates)
                {
                    var annotationRectangle = new Rectangle(boxToDraw.X1, boxToDraw.Y1, boxToDraw.X2, boxToDraw.Y2);
                    var redactionAnnotation = new RedactionAnnotation(annotationPage, annotationRectangle)
                    {
                        FillColor = Color.Black
                    };

                    doc.Pages[currentPage].Annotations.Add(redactionAnnotation, true);
                    redactionAnnotation.Redact();
                }
            }
            doc.RemoveMetadata();

            using var redactedDocumentStream = new MemoryStream();
            doc.Save(redactedDocumentStream);
            
            await _blobStorageService.UploadDocumentAsync(redactedDocumentStream, newFileName);

            saveResult.Succeeded = true;
            saveResult.RedactedDocumentName = newFileName;

            return saveResult;
        }
    }
}
