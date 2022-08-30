using Aspose.Pdf.Facades;
using pdf_generator.Domain.Redaction;

namespace pdf_generator.Services.DocumentRedactionService
{
    public class CoordinateCalculator : ICoordinateCalculator
    {
        public RedactionCoordinates CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinates originatorCoordinates, PdfFileInfo targetPdfInfo)
        {
            var pdfTranslatedCoordinates = new RedactionCoordinates();
            var x1Cent = originatorCoordinates.X1 * 100 / pageWidth;
            var y1Cent = originatorCoordinates.Y1 * 100 / pageHeight;
            var x2Cent = originatorCoordinates.X2 * 100 / pageWidth;
            var y2Cent = originatorCoordinates.Y2 * 100 / pageHeight;

            var pdfWidth = targetPdfInfo.GetPageWidth(pageIndex);
            var pdfHeight = targetPdfInfo.GetPageHeight(pageIndex);

            pdfTranslatedCoordinates.X1 = pdfWidth / 100 * x1Cent;
            pdfTranslatedCoordinates.Y1 = pdfHeight / 100 * y1Cent;
            pdfTranslatedCoordinates.X2 = pdfWidth / 100 * x2Cent;
            pdfTranslatedCoordinates.Y2 = pdfHeight / 100 * y2Cent;

            return pdfTranslatedCoordinates;
        }
    }
}
