using System.Text;
using FluentAssertions;
using pdf_generator.Factories;
using Xunit;

namespace rumpolepipeline.tests.pdf_generator.Factories
{
    public class AsposeItemFactoryTests
    {
        private readonly IAsposeItemFactory _asposeItemFactory;

        public AsposeItemFactoryTests()
        {
            _asposeItemFactory = new AsposeItemFactory();
        }

        [Fact]
        public void CreateWorkbook_ReturnsValidObject()
        {
            using var testStream = GetType().Assembly.GetManifestResourceStream("rumpolepipeline.tests.pdf_generator.TestResources.TestBook.xlsx");
            var result = _asposeItemFactory.CreateWorkbook(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateDiagram_ReturnsValidObject()
        {
            using var testStream = GetType().Assembly.GetManifestResourceStream("rumpolepipeline.tests.pdf_generator.TestResources.TestDiagram.vsd");
            var result = _asposeItemFactory.CreateDiagram(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateMailMessage_ReturnsValidObject()
        {
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
            var result = _asposeItemFactory.CreateMailMessage(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateMhtmlDocument_ReturnsValidObject()
        {
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
            var result = _asposeItemFactory.CreateMhtmlDocument(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateHtmlDocument_ReturnsValidObject()
        {
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
            var result = _asposeItemFactory.CreateHtmlDocument(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateImage_ReturnsValidObject()
        {
            using var testStream = GetType().Assembly.GetManifestResourceStream("rumpolepipeline.tests.pdf_generator.TestResources.TestImage.png");
            var result = _asposeItemFactory.CreateImage(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreatePresentation_ReturnsValidObject()
        {
            using var testStream = GetType().Assembly.GetManifestResourceStream("rumpolepipeline.tests.pdf_generator.TestResources.TestPresentation.pptx");
            var result = _asposeItemFactory.CreatePresentation(testStream);

            result.Should().NotBeNull();
        }

        [Fact]
        public void CreateWords_ReturnsValidObject()
        {
            using var testStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));
            var result = _asposeItemFactory.CreateWordsDocument(testStream);

            result.Should().NotBeNull();
        }
    }
}
