using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Domain.SearchResults;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentEvaluationService;
using Xunit;

namespace pdf_generator.tests.Services.DocumentEvaluationService;

public class DocumentEvaluationServiceTests
{
    private readonly Mock<IBlobStorageService> _mockBlobStorageService;
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;
    private readonly BlobSearchResult _convertedBlob;

    private readonly IDocumentEvaluationService _documentEvaluationService;

    public DocumentEvaluationServiceTests()
    {
        _fixture = new Fixture();
        _mockBlobStorageService = new Mock<IBlobStorageService>();
        var mockLogger = new Mock<ILogger<pdf_generator.Services.DocumentEvaluationService.DocumentEvaluationService>>();

        _correlationId = Guid.NewGuid();

        var incomingDocument = _fixture.Create<CaseDocument>();

        _documentEvaluationService = new pdf_generator.Services.DocumentEvaluationService.DocumentEvaluationService(_mockBlobStorageService.Object, mockLogger.Object);

        _mockBlobStorageService.Setup(s => s.RemoveDocumentAsync(It.IsAny<string>(), _correlationId)).ReturnsAsync(true);

        _convertedBlob = new BlobSearchResult {BlobName = incomingDocument.FileName, VersionId = incomingDocument.VersionId};
        
        _mockBlobStorageService.Setup(x => x.FindBlobsByPrefixAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<BlobSearchResult> { _convertedBlob });
    }

    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsNotFoundInBlobStorage_ShouldAcquireDocument()
    {
        var request = _fixture.Create<EvaluateDocumentRequest>();
        _mockBlobStorageService.Setup(x => x.FindBlobsByPrefixAsync(It.IsAny<string>(), It.IsAny<Guid>()))
            .ReturnsAsync(new List<BlobSearchResult>());
        
        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId.ToString());
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.AcquireDocument);
            result.UpdateSearchIndex.Should().BeFalse();
            
            _mockBlobStorageService.Verify(v => v.RemoveDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }
    }
    
    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsMatchedExactlyToBlobStorage_ShouldLeaveTheDocumentUnchanged()
    {
        var request = _fixture.Create<EvaluateDocumentRequest>();
        var storedDocument = new BlobSearchResult
        {
            BlobName = request.ProposedBlobName,
            VersionId = request.VersionId
        };
        
        _mockBlobStorageService.Setup(s => s.FindBlobsByPrefixAsync(It.IsAny<string>(), _correlationId))
            .ReturnsAsync(new List<BlobSearchResult> { storedDocument });

        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId.ToString());
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.DocumentUnchanged);
            result.UpdateSearchIndex.Should().BeFalse();
            
            _mockBlobStorageService.Verify(v => v.RemoveDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }
    }
    
    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsNotMatchedExactlyToBlobStorage_ByVersionId_ShouldAcquireTheNewDocument_AndUpdateTheSearchIndexToRemoveTheOld()
    {
        var request = _fixture.Create<EvaluateDocumentRequest>();
        var storedDocument = new BlobSearchResult
        {
            BlobName = _fixture.Create<string>(),
            VersionId = _fixture.Create<long>()
        };
        
        _mockBlobStorageService.Setup(s => s.FindBlobsByPrefixAsync(It.IsAny<string>(), _correlationId))
            .ReturnsAsync(new List<BlobSearchResult> { storedDocument });

        var result = await _documentEvaluationService.EvaluateDocumentAsync(request, _correlationId);
        
        using (new AssertionScope())
        {
            result.CaseId.Should().Be(request.CaseId.ToString());
            result.DocumentId.Should().Be(request.DocumentId);
            result.EvaluationResult.Should().Be(DocumentEvaluationResult.AcquireDocument);
            result.UpdateSearchIndex.Should().BeTrue();
            
            _mockBlobStorageService.Verify(v => v.RemoveDocumentAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
