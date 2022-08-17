using Azure.Storage.Sas;
using FluentAssertions;
using text_extractor.Factories;
using text_extractor.Wrappers;
using Xunit;

namespace rumpolepipeline.tests.text_extractor.Factories
{
    public class BlobSasBuilderWrapperFactoryTests
    {
        [Fact]
        public void Create_ReturnsBlobSasBuilderWrapper()
        {
            var factory = new BlobSasBuilderWrapperFactory();

            var blobSasBuilder = factory.Create(new BlobSasBuilder());

            blobSasBuilder.Should().BeOfType<BlobSasBuilderWrapper>();
        }
    }
}
