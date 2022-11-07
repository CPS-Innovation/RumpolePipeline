using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspose.Cells;
using AutoFixture;
using Common.Constants;
using Common.Domain.Redaction;
using Common.Domain.Requests;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Factories;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentRedactionService;
using pdf_generator.Services.PdfService;
using Xunit;

namespace pdf_generator.tests.Services.DocumentRedactionService;

public class DocumentRedactionServiceTests
{
    private readonly Mock<IBlobStorageService> _mockBlobStorageService;

    private readonly IDocumentRedactionService _documentRedactionService;

    private readonly RedactPdfRequest _redactPdfRequest;
    private readonly string _accessToken;
    private readonly Guid _correlationId;
    private readonly MemoryStream _pdfStream;

    public DocumentRedactionServiceTests()
    {
        var fixture = new Fixture();
        _mockBlobStorageService = new Mock<IBlobStorageService>();
        var mockLogger = new Mock<ILogger<pdf_generator.Services.DocumentRedactionService.DocumentRedactionService>>();
        var mockCalculatorLogger = new Mock<ILogger<CoordinateCalculator>>();
        ICoordinateCalculator coordinateCalculator = new CoordinateCalculator(mockCalculatorLogger.Object);
        
        var asposeItemFactory = new Mock<IAsposeItemFactory>();
        asposeItemFactory.Setup(x => x.CreateWorkbook(It.IsAny<Stream>(), It.IsAny<Guid>())).Returns(new Workbook());

        IPdfService pdfService = new CellsPdfService(asposeItemFactory.Object);
        
        var configuration = new Mock<IConfiguration>();

        configuration.Setup(x => x[FeatureFlags.EvaluateDocuments]).Returns("true");

        _documentRedactionService = new pdf_generator.Services.DocumentRedactionService.DocumentRedactionService(_mockBlobStorageService.Object,
            coordinateCalculator, mockLogger.Object, configuration.Object);

        _redactPdfRequest = fixture.Create<RedactPdfRequest>();
        _redactPdfRequest.RedactionDefinitions = fixture.CreateMany<RedactionDefinition>(1).ToList();
        _redactPdfRequest.RedactionDefinitions[0].PageIndex = 1;

        _accessToken = fixture.Create<string>();
        _correlationId = Guid.NewGuid();
        
        _pdfStream = new MemoryStream();
        using var inputStream = GetType().Assembly.GetManifestResourceStream("pdf_generator.tests.TestResources.TestBook.xlsx");
        
        pdfService.ReadToPdfStream(inputStream, _pdfStream, Guid.NewGuid());

        _mockBlobStorageService.Setup(s => s.GetDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(_pdfStream);

        _mockBlobStorageService.Setup(s => s.UploadDocumentAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task WhenDocumentNotFoundInBlobStorage_HandledAndSaveResultReturned()
    {
        _mockBlobStorageService.Setup(s => s.GetDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync((Stream)null);

        var saveResult = await _documentRedactionService.RedactPdfAsync(_redactPdfRequest, _accessToken, _correlationId);
        
        using (new AssertionScope())
        {
            saveResult.Succeeded.Should().BeFalse();
            saveResult.Message.Should().Be($"Invalid document - a document with filename '{_redactPdfRequest.FileName}' could not be retrieved for redaction purposes");
        }
    }
}
