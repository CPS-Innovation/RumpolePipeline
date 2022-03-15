using System;
using System.IO;
using Aspose.Cells;

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
                //throw new Exception($"Failed to set Aspose License: {exception.Message}");
            }
        }

        public void ReadToPdfStream(Stream inputStream, Stream pdfStream)
        {
            using var workbook = new Workbook(inputStream);
            workbook.Save(pdfStream, SaveFormat.Pdf); //TODO need to save in specific format? (like Slides)
        }
    }
}
