using System;
using System.IO;
using System.Threading.Tasks;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Devices;
using Aspose.Pdf.Facades;
using pdf_generator.Domain.Requests;
using pdf_generator.Domain.Responses;
using pdf_generator.Services.BlobStorageService;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class DocumentRedactionService : IDocumentRedactionService
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ICoordinateCalculator _coordinateCalculator;

        public DocumentRedactionService(IBlobStorageService blobStorageService, ICoordinateCalculator coordinateCalculator)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _coordinateCalculator = coordinateCalculator ?? throw new ArgumentNullException(nameof(coordinateCalculator));
        }

        public async Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken)
        {
            var saveResult = new RedactPdfResponse();

            //1. Load PDF from BLOB storage
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

            //2. Apply UI instructions by drawing boxes according to co-ordinate data onto existing PDF
            using var redactedDocument = new Document(document);
            var pdfInfo = new PdfFileInfo(redactedDocument);

            foreach (var redactionPage in redactPdfRequest.RedactionDefinitions)
            {
                var currentPage = redactionPage.PageIndex;
                var annotationPage = redactedDocument.Pages[currentPage];
                
                foreach (var boxToDraw in redactionPage.RedactionCoordinates)
                {
                    var translatedCoordinates = _coordinateCalculator.CalculateRelativeCoordinates(redactionPage.Width,
                        redactionPage.Height, currentPage, boxToDraw, pdfInfo);

                    var annotationRectangle = new Rectangle(translatedCoordinates.X1, translatedCoordinates.Y1, translatedCoordinates.X2, translatedCoordinates.Y2);
                    var redactionAnnotation = new RedactionAnnotation(annotationPage, annotationRectangle)
                    {
                        FillColor = Color.Black
                    };

                    redactedDocument.Pages[currentPage].Annotations.Add(redactionAnnotation, true);
                    redactionAnnotation.Redact();
                }
            }
            redactedDocument.RemoveMetadata();

            //3. now flatten the redacted document to remove "hidden" tags and data by converting each page to a Bitmap and then adding it back as an ASPOSE PDF page to a new PDF document, built up in-memory
            using var flattenedDocumentStream = new MemoryStream();
            using var flattenedDoc = new Document();
            var pos = 0;
            foreach (var page in redactedDocument.Pages)
            {
                pos++;
    
                using var imageMs = new MemoryStream();
                var currentPageWidth = pdfInfo.GetPageWidth(pos);
                var currentPageHeight = pdfInfo.GetPageHeight(pos);
                //var resolution = new Resolution(Convert.ToInt32(page.ArtBox.Width), Convert.ToInt32(page.ArtBox.Height));
                //var resolution = new Resolution(300);
                //var device = new BmpDevice(new PageSize(currentPageWidth, currentPageHeight), resolution);
                var device = new BmpDevice(new PageSize(currentPageWidth, currentPageHeight)); //default resolution

                // Convert a particular page and save the image to stream
                device.Process(page, imageMs);
    
                var newPage = flattenedDoc.Pages.Add();
                // Set margins so image will fit, etc.
                newPage.PageInfo.Margin.Bottom = 0;
                newPage.PageInfo.Margin.Top = 0;    
                newPage.PageInfo.Margin.Left = 0;
                newPage.PageInfo.Margin.Right = 0;


                // Create an image object
                var image1 = new Image
                {
                    // Set the image file stream
                    ImageStream = imageMs,
                    IsBlackWhite = true
                };

                // Add the image into paragraphs collection of the section
                newPage.Paragraphs.Add(image1);

                // YES, save after every page otherwise we get an out of memory exception
                flattenedDoc.Save(flattenedDocumentStream);
            }

            //4. Save the flattened PDF into BLOB storage but with a new filename (FOR NOW, until we have a tactical or "for-reals" API solution/integration in place)
            await _blobStorageService.UploadDocumentAsync(flattenedDocumentStream, newFileName);

            saveResult.Succeeded = true;
            saveResult.RedactedDocumentName = newFileName;

            return saveResult;
        }
    }
}