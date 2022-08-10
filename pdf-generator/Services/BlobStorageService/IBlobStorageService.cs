using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<Stream> GetDocumentAsync(string blobName);

        Task UploadDocumentAsync(Stream stream, string blobName);
    }
}
