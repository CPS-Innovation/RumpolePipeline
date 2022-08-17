using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Moq;
using pdf_generator.Services.BlobStorageService;
using Xunit;

namespace rumpolepipeline.tests.pdf_generator.Services.BlobStorageService
{
	public class BlobStorageServiceTests
	{
        private readonly Stream _stream;
		private readonly string _blobName;

        private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
		private readonly Mock<BlobClient> _mockBlobClient;

		private readonly IBlobStorageService _blobStorageService;

		public BlobStorageServiceTests()
		{
            var fixture = new Fixture();
			var blobContainerName = fixture.Create<string>();
			_stream = new MemoryStream();
			_blobName = fixture.Create<string>();

			var mockBlobServiceClient = new Mock<BlobServiceClient>();
			var mockBlobContainerClient = new Mock<BlobContainerClient>();
			_mockBlobClient = new Mock<BlobClient>();

			mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerName))
				.Returns(mockBlobContainerClient.Object);

			_mockBlobContainerExistsResponse = new Mock<Response<bool>>();
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(true);
			mockBlobContainerClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_mockBlobContainerExistsResponse.Object);
			mockBlobContainerClient.Setup(client => client.GetBlobClient(_blobName)).Returns(_mockBlobClient.Object);

			_blobStorageService = new global::pdf_generator.Services.BlobStorageService.BlobStorageService(mockBlobServiceClient.Object, blobContainerName);
		}

		[Fact]
		public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.UploadDocumentAsync(_stream, _blobName));
		}

		[Fact]
		public async Task UploadDocumentAsync_UploadsDocument()
		{
			await _blobStorageService.UploadDocumentAsync(_stream, _blobName);

			_mockBlobClient.Verify(client => client.UploadAsync(_stream, true, It.IsAny<CancellationToken>()));
		}
	}
}

