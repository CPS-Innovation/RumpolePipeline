using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.DocumentEvaluation;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.BlobStorageService;
using pdf_generator.Services.DocumentEvaluationService;
using pdf_generator.Services.SearchService;
using Xunit;

namespace pdf_generator.tests.Services.DocumentEvaluationService;

public class DocumentEvaluationServiceTests
{
    private readonly Mock<IBlobStorageService> _mockBlobStorageService;
    private readonly Mock<ISearchService> _mockSearchService;
    private readonly Fixture _fixture;
    private readonly string _caseId;
    private readonly Guid _correlationId;
    private readonly List<DocumentInformation> _documentsForCase;
    private readonly List<CaseDocument> _incomingDocuments;

    private readonly IDocumentEvaluationService _documentEvaluationService;

    public DocumentEvaluationServiceTests()
    {
        _fixture = new Fixture();
        _mockBlobStorageService = new Mock<IBlobStorageService>();
        _mockSearchService = new Mock<ISearchService>();
        var mockLogger = new Mock<ILogger<pdf_generator.Services.DocumentEvaluationService.DocumentEvaluationService>>();

        _caseId = _fixture.Create<string>();
        _correlationId = Guid.NewGuid();

        _documentsForCase = _fixture.CreateMany<DocumentInformation>(4).ToList();
        _incomingDocuments = _fixture.CreateMany<CaseDocument>(4).ToList();

        _documentEvaluationService = new pdf_generator.Services.DocumentEvaluationService.DocumentEvaluationService(_mockBlobStorageService.Object, _mockSearchService.Object, mockLogger.Object);

        _mockSearchService.Setup(s => s.ListDocumentsForCaseAsync(_caseId, _correlationId)).ReturnsAsync(_documentsForCase);
        _mockBlobStorageService.Setup(s => s.RemoveDocumentAsync(It.IsAny<string>(), _correlationId)).ReturnsAsync(true);
    }

    [Fact]
    public async Task EvaluateExistingDocumentsAsync_WhenNoDocumentsFoundForCase_ReturnsEmptyEvaluationResultsCollection()
    {
        _documentsForCase.Clear();
        _mockSearchService.Setup(s => s.ListDocumentsForCaseAsync(_caseId, _correlationId)).ReturnsAsync(_documentsForCase);

        var listResult = await _documentEvaluationService.EvaluateExistingDocumentsAsync(_caseId, _incomingDocuments, _correlationId);

        using (new AssertionScope())
        {
            listResult.Count.Should().Be(0);
            _mockSearchService.Verify(v => v.ListDocumentsForCaseAsync(It.IsAny<string>(), _correlationId), Times.Exactly(1));
        }
    }

    [Fact]
    public async Task EvaluateExistingDocumentsAsync_WhenIncomingDocuments_EntirelyMatch_DocumentsFoundForCase_ReturnsEmptyEvaluationResultsCollection()
    {
        _documentsForCase.Clear();
        foreach (var incomingDoc in _incomingDocuments)
        {
            var blobItemWrapper = new DocumentInformation
            {
                DocumentMetadata = new Dictionary<string, string>(),
                BlobName = incomingDoc.FileName,
                BlobContainerName = _fixture.Create<string>()
            };
            blobItemWrapper.DocumentMetadata.Add(DocumentTags.DocumentId, incomingDoc.DocumentId);
            blobItemWrapper.DocumentMetadata.Add(DocumentTags.VersionId, incomingDoc.VersionId.ToString());
            
            _documentsForCase.Add(blobItemWrapper);
        }
        _mockSearchService.Setup(s => s.ListDocumentsForCaseAsync(_caseId, _correlationId)).ReturnsAsync(_documentsForCase);
        
        var listResult = await _documentEvaluationService.EvaluateExistingDocumentsAsync(_caseId, _incomingDocuments, _correlationId);

        using (new AssertionScope())
        {
            listResult.Count.Should().Be(0);
            _mockSearchService.Verify(v => v.ListDocumentsForCaseAsync(It.IsAny<string>(), _correlationId), Times.Exactly(1));
        }
    }
    
    [Fact]
    public async Task EvaluateExistingDocumentsAsync_WhenIncomingDocuments_DoNotEntirelyMatch_DocumentsFoundForCase_ReturnsPopulatedEvaluationResultsCollection_AndSearchIndexUpdated()
    {
        _documentsForCase.Clear();
        var pos = 1;
        foreach (var incomingDoc in _incomingDocuments)
        {
            var blobItemWrapper = new DocumentInformation
            {
                DocumentMetadata = new Dictionary<string, string>(),
                BlobName = incomingDoc.FileName,
                BlobContainerName = _fixture.Create<string>()
            };
            blobItemWrapper.DocumentMetadata.Add(DocumentTags.DocumentId, pos % 2 == 0 ? incomingDoc.DocumentId : _fixture.Create<string>());
            blobItemWrapper.DocumentMetadata.Add(DocumentTags.VersionId, incomingDoc.VersionId.ToString());
            
            _documentsForCase.Add(blobItemWrapper);
            pos++;
        }
        _mockSearchService.Setup(s => s.ListDocumentsForCaseAsync(_caseId, _correlationId)).ReturnsAsync(_documentsForCase);
        
        var listResult = await _documentEvaluationService.EvaluateExistingDocumentsAsync(_caseId, _incomingDocuments, _correlationId);

        using (new AssertionScope())
        {
            listResult.Count.Should().Be(2);
            _mockSearchService.Verify(v => v.ListDocumentsForCaseAsync(It.IsAny<string>(), _correlationId), Times.Exactly(1));
            _mockBlobStorageService.Verify(v => v.RemoveDocumentAsync(It.IsAny<string>(), _correlationId), Times.Exactly(2));
        }
    }

    [Fact]
    public async Task EvaluateDocumentAsync_WhenDocumentIsNotFoundInBlobStorage_ShouldAcquireDocument()
    {
        var request = _fixture.Create<EvaluateDocumentRequest>();
        _mockSearchService.Setup(s => s.FindDocumentForCaseAsync(request.CaseId.ToString(), request.DocumentId, _correlationId))
            .ReturnsAsync((DocumentInformation) null);

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
        var storedDocument = new DocumentInformation
        {
            DocumentMetadata = new Dictionary<string, string>(),
            BlobName = _fixture.Create<string>(),
            BlobContainerName = _fixture.Create<string>()
        };
        storedDocument.DocumentMetadata.Add(DocumentTags.VersionId, request.VersionId.ToString());
        
        _mockSearchService.Setup(s => s.FindDocumentForCaseAsync(request.CaseId.ToString(), request.DocumentId, _correlationId))
            .ReturnsAsync(storedDocument);

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
        var storedDocument = new DocumentInformation
        {
            DocumentMetadata = new Dictionary<string, string>(),
            BlobName = _fixture.Create<string>(),
            BlobContainerName = _fixture.Create<string>()
        };
        storedDocument.DocumentMetadata.Add(DocumentTags.VersionId, _fixture.Create<long>().ToString());
        
        _mockSearchService.Setup(s => s.FindDocumentForCaseAsync(request.CaseId.ToString(), request.DocumentId, _correlationId))
            .ReturnsAsync(storedDocument);

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
