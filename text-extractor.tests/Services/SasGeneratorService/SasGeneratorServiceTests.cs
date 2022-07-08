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
        private readonly string _blobName;
        private readonly BlobUriBuilder _blobUriBuilder;

        private readonly ISasGeneratorService _sasGeneratorService;

        public SasGeneratorServiceTests()
        {
            var fixture = new Fixture();
            var blobOptions = fixture.Create<BlobOptions>();
            _blobName = fixture.Create<string>();
            var blobSasBuilder = fixture.Create<BlobSasBuilder>();

            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockBlobSasBuilderFactory = new Mock<IBlobSasBuilderFactory>();
            var mockBlobSasBuilderWrapperFactory = new Mock<IBlobSasBuilderWrapperFactory>();
            var mockBlobOptions = new Mock<IOptions<BlobOptions>>();
            var mockResponse = new Mock<Response<UserDelegationKey>>();
            var mockUserDelegationKey = new Mock<UserDelegationKey>();
            var mockBlobSasBuilderWrapper = new Mock<IBlobSasBuilderWrapper>();

            mockBlobOptions.Setup(options => options.Value).Returns(blobOptions);
            mockResponse.Setup(response => response.Value).Returns(mockUserDelegationKey.Object);
            mockBlobServiceClient.Setup(client => client.GetUserDelegationKeyAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);
            mockBlobServiceClient.Setup(client => client.Uri).Returns(fixture.Create<Uri>());

            _blobUriBuilder = new BlobUriBuilder(new Uri($"{mockBlobServiceClient.Object.Uri}{blobOptions.BlobContainerName}/{_blobName}"));
            mockBlobSasBuilderFactory.Setup(factory => factory.Create(_blobUriBuilder.BlobName)).Returns(blobSasBuilder);
            mockBlobSasBuilderWrapper.Setup(wrapper => wrapper.ToSasQueryParameters(mockUserDelegationKey.Object, mockBlobServiceClient.Object.AccountName))
                .Returns(new Mock<SasQueryParameters>().Object.As<BlobSasQueryParameters>());
            mockBlobSasBuilderWrapperFactory.Setup(factory => factory.Create(blobSasBuilder)).Returns(mockBlobSasBuilderWrapper.Object);

            _sasGeneratorService = new text_extractor.Services.SasGeneratorService.SasGeneratorService(mockBlobServiceClient.Object, mockBlobSasBuilderFactory.Object, mockBlobSasBuilderWrapperFactory.Object, mockBlobOptions.Object);
        }

        [Fact]
        public async Task GenerateSasUrl_ReturnsExpectedUri()
        {
            var response = await _sasGeneratorService.GenerateSasUrlAsync(_blobName);

            response.Should().Be(_blobUriBuilder.ToUri().ToString());
        }
    }
}
