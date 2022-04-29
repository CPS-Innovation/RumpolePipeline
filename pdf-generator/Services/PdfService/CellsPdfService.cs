using System;
using System.IO;
using Aspose.Cells;
using pdf_generator.Domain.Exceptions;

namespace pdf_generator.Services.PdfService
{
    public class CellsPdfService : IPdfService
    {
        public CellsPdfService()
        {
            try
            {
                var license = new License();
                license.SetLicense("Aspose.Total.NET.lic");
            }
            catch (Exception exception)
            {
                throw new AsposeLicenseException(exception.Message);
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            using var workbook = new Workbook(inputStream);
            workbook.Save(pdfStream, new PdfSaveOptions { OnePagePerSheet = true });
            pdfStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
