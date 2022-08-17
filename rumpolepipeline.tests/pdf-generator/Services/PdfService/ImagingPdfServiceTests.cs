using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.PdfService;
using Xunit;

namespace rumpolepipeline.tests.pdf_generator.Services.PdfService
{
    public class ImagingPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public ImagingPdfServiceTests()
        {
            using var testImageStream = GetType().Assembly.GetManifestResourceStream("rumpolepipeline.tests.pdf_generator.TestResources.TestImage.png");

            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateImage(It.IsAny<Stream>())).Returns(Aspose.Imaging.Image.Load(testImageStream));
            _pdfService = new ImagingPdfService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new ImagingPdfService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }

        [Fact]
        public void ReadToPdfStream_CallsCreateImage()
        {
            using var pdfStream = new MemoryStream();
            using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes("whatever"));

            _pdfService.ReadToPdfStream(inputStream, pdfStream);

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateImage(It.IsAny<Stream>()));
                pdfStream.Should().NotBeNull();
                pdfStream.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
