using System.IO;

namespace pdf_generator.Services.PdfService
{
    public interface IPdfOrchestratorService
    {
        Stream ReadToPdfStream(Stream inputStream, string fileType);
    }
}
