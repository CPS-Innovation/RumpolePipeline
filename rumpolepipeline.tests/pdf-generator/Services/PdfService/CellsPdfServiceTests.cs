using Aspose.Cells;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.PdfService;
using Xunit;

namespace rumpolepipeline.tests.pdf_generator.Services.PdfService
{
    public class CellsPdfServiceTests
    {
        private readonly Mock<IAsposeItemFactory> _asposeItemFactory;
        private readonly IPdfService _pdfService;

        public CellsPdfServiceTests()
        {
            _asposeItemFactory = new Mock<IAsposeItemFactory>();
            _asposeItemFactory.Setup(x => x.CreateWorkbook(It.IsAny<Stream>())).Returns(new Workbook());

            _pdfService = new CellsPdfService(_asposeItemFactory.Object);
        }

        [Fact]
        public void Ctor_NoItemFactory_ThrowsAppropriateException()
        {
            var act = () =>
            {
                var _ = new CellsPdfService(null);
            };

            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("asposeItemFactory");
        }

        [Fact]
        public void ReadToPdfStream_CallsCreateWorkbook()
        {
            using var pdfStream = new MemoryStream();
            using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestBook.xlsx");

            _pdfService.ReadToPdfStream(inputStream, pdfStream);

            using (new AssertionScope())
            {
                _asposeItemFactory.Verify(x => x.CreateWorkbook(It.IsAny<Stream>()));
                pdfStream.Should().NotBeNull();
                pdfStream.Length.Should().BeGreaterThan(0);
            }
        }
    }
}
