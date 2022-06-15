using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using text_extractor.Factories;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Factories
{
    public class BlobSasBuilderFactoryTests
    {
        private Fixture _fixture;
        private BlobOptions _blobOptions;
        private string _blobName;

        private Mock<IOptions<BlobOptions>> _mockBlobOptions;

        private IBlobSasBuilderFactory BlobSasBuilderFactory;

        public BlobSasBuilderFactoryTests()
        {
            _fixture = new Fixture();
            _blobOptions = _fixture.Create<BlobOptions>();
            _blobName = _fixture.Create<string>();

            _mockBlobOptions = new Mock<IOptions<BlobOptions>>();

            _mockBlobOptions.Setup(options => options.Value).Returns(_blobOptions);

            BlobSasBuilderFactory = new BlobSasBuilderFactory(_mockBlobOptions.Object);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedBlobContainerName()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.BlobContainerName.Should().Be(_blobOptions.BlobContainerName);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedBlobName()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.BlobName.Should().Be(_blobName);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedResource()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.Resource.Should().Be("b");
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithStartTimeBeforeNow()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.StartsOn.Should().BeBefore(DateTimeOffset.UtcNow);
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedExpiresOn()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.ExpiresOn.Should().Be(sasBuilder.StartsOn.AddSeconds(_blobOptions.BlobExpirySecs));
        }

        [Fact]
        public void Create_ReturnsSasBuilderWithExpectedPermissions()
        {
            var sasBuilder = BlobSasBuilderFactory.Create(_blobName);

            sasBuilder.Permissions.Should().Be("r");
        }
    }
}
