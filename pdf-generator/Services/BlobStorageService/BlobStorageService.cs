using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace pdf_generator.Services.BlobStorageService
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobStorageOptions _options;
        public BlobStorageService(IOptions<BlobStorageOptions> options)
        {
            _options = options.Value;
        }

        public async Task<Stream> DownloadDocumentAsync(string documentSasUrl)
        {
            var blobClient = CreateBlobClient(new Uri(documentSasUrl));

            var result = await blobClient.DownloadContentAsync();

            return result.Value.Content.ToStream();
        }

        public async Task UploadAsync(Stream stream, string blobName)
        {
            var blobClient = CreateBlobClient(blobName);

            await blobClient.UploadAsync(stream, true);
        }

        private BlobClient CreateBlobClient(Uri documentSasUrl)
        {
            return new BlobClient(documentSasUrl);
        }

        private BlobClient CreateBlobClient(string blobName)
        {
            return new BlobClient(_options.ConnectionString, _options.ContainerName, blobName);
        }
    }
}