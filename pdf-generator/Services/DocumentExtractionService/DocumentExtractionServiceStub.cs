using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace pdf_generator.Services.DocumentExtractionService
{
	public class DocumentExtractionServiceStub : IDocumentExtractionService
	{
        private readonly string _blobStorageConnectionString;

        public DocumentExtractionServiceStub(string blobStorageConnectionString)
		{
            _blobStorageConnectionString = blobStorageConnectionString;
        }

        public async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken)
        {
            var blobClient = new BlobClient(_blobStorageConnectionString, "cms-documents", fileName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            var blob = await blobClient.DownloadContentAsync();

            return blob.Value.Content.ToStream();
        }
    }
}

