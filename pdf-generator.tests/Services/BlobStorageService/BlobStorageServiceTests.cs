using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Moq;
using pdf_generator.Services.BlobStorageService;
using Xunit;

namespace pdf_generator.tests.Services.BlobStorageService
{
	public class BlobStorageServiceTests
	{
		private Fixture _fixture;
		private string _blobContainerName;
		private Stream _stream;
		private string _blobName;

		private Mock<BlobServiceClient> _mockBlobServiceClient;
		private Mock<BlobContainerClient> _mockBlobContainerClient;
		private Mock<Response<bool>> _mockBlobContainerExistsResponse;
		private Mock<BlobClient> _mockBlobClient;

		private IBlobStorageService BlobStorageService;

		public BlobStorageServiceTests()
		{
			_fixture = new Fixture();
			_blobContainerName = _fixture.Create<string>();
			_stream = new MemoryStream();
			_blobName = _fixture.Create<string>();

			_mockBlobServiceClient = new Mock<BlobServiceClient>();
			_mockBlobContainerClient = new Mock<BlobContainerClient>();
			_mockBlobClient = new Mock<BlobClient>();

			_mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(_blobContainerName))
				.Returns(_mockBlobContainerClient.Object);

			_mockBlobContainerExistsResponse = new Mock<Response<bool>>();
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(true);
			_mockBlobContainerClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_mockBlobContainerExistsResponse.Object);
			_mockBlobContainerClient.Setup(client => client.GetBlobClient(_blobName)).Returns(_mockBlobClient.Object);

			BlobStorageService = new pdf_generator.Services.BlobStorageService.BlobStorageService(_mockBlobServiceClient.Object, _blobContainerName);
		}

		[Fact]
		public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => BlobStorageService.UploadDocumentAsync(_stream, _blobName));
		}

		[Fact]
		public async Task UploadDocumentAsync_UploadsDocument()
		{
			await BlobStorageService.UploadDocumentAsync(_stream, _blobName);

			_mockBlobClient.Verify(client => client.UploadAsync(_stream));
		}
	}
}

