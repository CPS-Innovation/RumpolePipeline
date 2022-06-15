using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Factories;
using text_extractor.Services.SasGeneratorService;
using text_extractor.Services.SearchIndexService;
using text_extractor.Wrappers;
using Xunit;

namespace text_extractor.tests.Services.SasGeneratorService
{
    public class SasGeneratorServiceTests
    {
        private Fixture _fixture;
        private BlobOptions _blobOptions;
        private string _blobName;
        private BlobUriBuilder _blobUriBuilder;
        private BlobSasBuilder _blobSasBuilder;

        private Mock<BlobServiceClient> _mockBlobServiceClient;
        private Mock<IBlobSasBuilderFactory> _mockBlobSasBuilderFactory;
        private Mock<IBlobSasBuilderWrapperFactory> _mockBlobSasBuilderWrapperFactory;
        private Mock<IOptions<BlobOptions>> _mockBlobOptions;
        private Mock<Response<UserDelegationKey>> _mockResponse;
        private Mock<UserDelegationKey> _mockUserDelegationKey;
        private Mock<IBlobSasBuilderWrapper> _mockBlobSasBuilderWrapper;

        private ISasGeneratorService SasGeneratorService;

        public SasGeneratorServiceTests()
        {
            _fixture = new Fixture();
            _blobOptions = _fixture.Create<BlobOptions>();
            _blobName = _fixture.Create<string>();
            _blobSasBuilder = _fixture.Create<BlobSasBuilder>();

            _mockBlobServiceClient = new Mock<BlobServiceClient>();
            _mockBlobSasBuilderFactory = new Mock<IBlobSasBuilderFactory>();
            _mockBlobSasBuilderWrapperFactory = new Mock<IBlobSasBuilderWrapperFactory>();
            _mockBlobOptions = new Mock<IOptions<BlobOptions>>();
            _mockResponse = new Mock<Response<UserDelegationKey>>();
            _mockUserDelegationKey = new Mock<UserDelegationKey>();
            _mockBlobSasBuilderWrapper = new Mock<IBlobSasBuilderWrapper>();

            _mockBlobOptions.Setup(options => options.Value).Returns(_blobOptions);
            _mockResponse.Setup(response => response.Value).Returns(_mockUserDelegationKey.Object);
            _mockBlobServiceClient.Setup(client => client.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockResponse.Object);
            _mockBlobServiceClient.Setup(client => client.Uri).Returns(_fixture.Create<Uri>());

            _blobUriBuilder = new BlobUriBuilder(new Uri($"{_mockBlobServiceClient.Object.Uri}{_blobOptions.BlobContainerName}/{_blobName}"));
            _mockBlobSasBuilderFactory.Setup(factory => factory.Create(_blobUriBuilder.BlobName)).Returns(_blobSasBuilder);
            _mockBlobSasBuilderWrapper.Setup(wrapper => wrapper.ToSasQueryParameters(_mockUserDelegationKey.Object, _mockBlobServiceClient.Object.AccountName))
                .Returns(new Mock<SasQueryParameters>().Object.As<BlobSasQueryParameters>());
            _mockBlobSasBuilderWrapperFactory.Setup(factory => factory.Create(_blobSasBuilder)).Returns(_mockBlobSasBuilderWrapper.Object);

            SasGeneratorService = new text_extractor.Services.SasGeneratorService.SasGeneratorService(_mockBlobServiceClient.Object, _mockBlobSasBuilderFactory.Object, _mockBlobSasBuilderWrapperFactory.Object, _mockBlobOptions.Object);
        }

        [Fact]
        public async Task GenerateSasUrl_ReturnsExpectedUri()
        {
            var response = await SasGeneratorService.GenerateSasUrlAsync(_blobName);

            response.Should().Be(_blobUriBuilder.ToUri().ToString());
        }
    }
}
