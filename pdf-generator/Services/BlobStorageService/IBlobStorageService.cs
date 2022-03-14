using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.BlobStorageService
{
    public interface IBlobStorageService
    {
        Task<Stream> DownloadDocumentAsync(string documentSasUrl);

        Task UploadAsync(Stream stream, string blobName);
    }
}
