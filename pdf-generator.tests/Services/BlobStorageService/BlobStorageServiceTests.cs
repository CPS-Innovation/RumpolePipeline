using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.BlobStorageService;
using Xunit;

namespace pdf_generator.tests.Services.BlobStorageService
{
	public class BlobStorageServiceTests
	{
		private readonly Fixture _fixture;
		private readonly Stream _stream;
		private readonly string _blobName;
		private readonly Guid _correlationId;
		private readonly string _caseId;
		private readonly string _documentId;
		private readonly string _materialId;
		private readonly string _lastUpdatedDate;

		private readonly Mock<Response<bool>> _mockBlobContainerExistsResponse;
		private readonly Mock<BlobContainerClient> _mockBlobContainerClient;
		private readonly Mock<BlobClient> _mockBlobClient;

		private readonly IBlobStorageService _blobStorageService;

		public BlobStorageServiceTests()
		{
			_fixture = new Fixture();
			var blobContainerName = _fixture.Create<string>();
			_stream = new MemoryStream();
			_blobName = _fixture.Create<string>();
			_correlationId = _fixture.Create<Guid>();
			_caseId = _fixture.Create<string>();
			_documentId = _fixture.Create<string>();
			_materialId = _fixture.Create<string>();
			_lastUpdatedDate = _fixture.Create<string>();

			var mockBlobServiceClient = new Mock<BlobServiceClient>();
			_mockBlobContainerClient = new Mock<BlobContainerClient>();
			_mockBlobClient = new Mock<BlobClient>();

			mockBlobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerName))
				.Returns(_mockBlobContainerClient.Object);

			_mockBlobContainerExistsResponse = new Mock<Response<bool>>();
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(true);
			_mockBlobContainerClient.Setup(client => client.ExistsAsync(It.IsAny<CancellationToken>()))
				.ReturnsAsync(_mockBlobContainerExistsResponse.Object);
			_mockBlobContainerClient.Setup(client => client.GetBlobClient(_blobName)).Returns(_mockBlobClient.Object);
			var mockLogger = new Mock<ILogger<pdf_generator.Services.BlobStorageService.BlobStorageService>>();

			_blobStorageService = new pdf_generator.Services.BlobStorageService.BlobStorageService(mockBlobServiceClient.Object, blobContainerName, mockLogger.Object);
		}

		[Fact]
		public async Task GetDocumentAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.GetDocumentAsync(_blobName, _correlationId));
		}

		[Fact]
		public async Task GetDocumentAsync_ReturnsNull_WhenBlobClientCannotBeFound()
		{
			_mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(false, null!));
			
			var result = await _blobStorageService.GetDocumentAsync(_blobName, _correlationId);
			
			result.Should().BeNull();
		}
		
		[Fact]
		public async Task UploadDocumentAsync_ThrowsRequestFailedExceptionWhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.UploadDocumentAsync(_stream, _blobName, _caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId));
		}

		[Fact]
		public async Task UploadDocumentAsync_UploadsDocument()
		{
			await _blobStorageService.UploadDocumentAsync(_stream, _blobName, _caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId);

			_mockBlobClient.Verify(client => client.UploadAsync(_stream, true, It.IsAny<CancellationToken>()));
		}
		
		[Fact]
		public async Task ListDocumentsForCaseAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.ListDocumentsForCaseAsync(_caseId, _correlationId));
		}

		[Fact]
		public async Task FindDocumentForCaseAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.FindDocumentForCaseAsync(_caseId, _documentId, _correlationId));
		}

		[Fact]
		public async Task RemoveDocumentAsync_ThrowsRequestFailedException_WhenBlobContainerDoesNotExist()
		{
			_mockBlobContainerExistsResponse.Setup(response => response.Value).Returns(false);

			await Assert.ThrowsAsync<RequestFailedException>(() => _blobStorageService.RemoveDocumentAsync(_blobName, _correlationId));
		}

		[Fact]
		public async Task RemoveDocumentAsync_ReturnsNull_WhenBlobClientCannotBeFound()
		{
			_mockBlobClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Response.FromValue(false, null!));
			
			var result = await _blobStorageService.RemoveDocumentAsync(_blobName, _correlationId);
			
			result.Should().BeTrue();
		}
	}
}

