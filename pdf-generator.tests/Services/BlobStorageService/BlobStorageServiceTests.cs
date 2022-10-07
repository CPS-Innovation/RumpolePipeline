using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.BlobStorageService;
using Xunit;

namespace pdf_generator.tests.Services.BlobStorageService
{
	public class BlobStorageServiceTests
	{
		private readonly Stream _stream;
		private readonly string _blobName;
		private readonly Guid _correlationId;

		private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
		private readonly Mock<BlobClient> _mockBlobClient;

		private readonly IBlobStorageService _blobStorageService;

		public BlobStorageServiceTests()
		{
			var fixture = new Fixture();
			var blobContainerName = fixture.Create<string>();
			_stream = new MemoryStream();
			_blobName = fixture.Create<string>();
			_correlationId = fixture.Create<Guid>();

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
			var mockLogger = new Mock<ILogger<pdf_generator.Services.BlobStorageService.BlobStorageService>>();

			_blobStorageService = new pdf_generator.Services.BlobStorageService.BlobStorageService(mockBlobServiceClient.Object, blobContainerName, mockLogger.Object);
		}

		[Fact]
		public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.UploadDocumentAsync(_stream, _blobName, _correlationId));
		}

		[Fact]
		public async Task UploadDocumentAsync_UploadsDocument()
		{
			await _blobStorageService.UploadDocumentAsync(_stream, _blobName, _correlationId);

			_mockBlobClient.Verify(client => client.UploadAsync(_stream, true, It.IsAny<CancellationToken>()));
		}
	}
}

