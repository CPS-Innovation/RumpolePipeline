using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Search.Documents;
using Common.Domain.DocumentEvaluation;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Services.SearchService;
using Xunit;

namespace pdf_generator.tests.Services.SearchService;

public class SearchServiceTests
{
    private readonly Fixture _fixture;
    private readonly string _caseId;
    private readonly string _documentId;
    private readonly Guid _correlationId;
    private readonly Mock<ISearchServiceProcessor> _searchServiceProcessorMock;

    private readonly ISearchService _searchService;
    
    public SearchServiceTests()
    {
        _fixture = new Fixture();
        _caseId = _fixture.Create<string>();
        _documentId = _fixture.Create<string>();
        _correlationId = Guid.NewGuid();
        _searchServiceProcessorMock = new Mock<ISearchServiceProcessor>();
        var mockLogger = new Mock<ILogger<pdf_generator.Services.SearchService.SearchService>>();

        _searchService = new pdf_generator.Services.SearchService.SearchService(mockLogger.Object, _searchServiceProcessorMock.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ListDocumentsForCaseAsync_WhenCaseIdIsInvalid_ThrowsArgumentNullException(string caseId)
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _searchService.ListDocumentsForCaseAsync(caseId, _correlationId));
    }

    [Fact]
    public async Task ListDocumentsForCaseAsync_WhenAValidCaseId_ReturnsDocumentsCollection()
    {
        _searchServiceProcessorMock.Setup(x => x.SearchForDocumentsAsync(It.IsAny<SearchOptions>(), _correlationId))
            .ReturnsAsync(_fixture.CreateMany<DocumentInformation>(3).ToList());

        var result = await _searchService.ListDocumentsForCaseAsync(_caseId, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
        }
    }
    
    [Fact]
    public async Task ListDocumentsForCaseAsync_WhenAnInValidCaseId_ReturnsZeroLengthDocumentsCollection()
    {
        _searchServiceProcessorMock.Setup(x => x.SearchForDocumentsAsync(It.IsAny<SearchOptions>(), _correlationId))
            .ReturnsAsync(new List<DocumentInformation>());

        var result = await _searchService.ListDocumentsForCaseAsync(_caseId, _correlationId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(0);
        }
    }
    
    [Theory]
    [InlineData("", "123")]
    [InlineData(" ", "123")]
    [InlineData(null, "123")]
    [InlineData("123", "")]
    [InlineData("123", " ")]
    [InlineData("123", null)]
    public async Task FindDocumentForCaseAsync_WhenCaseIdOrDocumentIdIsInvalid_ThrowsArgumentNullException(string caseId, string documentId)
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _searchService.FindDocumentForCaseAsync(caseId, documentId, _correlationId));
    }
    
    [Fact]
    public async Task FindDocumentForCaseAsync_WhenAValidCaseId_ReturnsASingleDocument()
    {
        _searchServiceProcessorMock.Setup(x => x.SearchForDocumentsAsync(It.IsAny<SearchOptions>(), _correlationId))
            .ReturnsAsync(_fixture.CreateMany<DocumentInformation>(3).ToList());

        var result = await _searchService.FindDocumentForCaseAsync(_caseId, _documentId, _correlationId);

        result.Should().NotBeNull();
    }
    
    [Fact]
    public async Task FindDocumentForCaseAsync_WhenAnInValidCaseId_ReturnsNullType()
    {
        _searchServiceProcessorMock.Setup(x => x.SearchForDocumentsAsync(It.IsAny<SearchOptions>(), _correlationId))
            .ReturnsAsync(new List<DocumentInformation>());

        var result = await _searchService.FindDocumentForCaseAsync(_caseId, _documentId, _correlationId);

        result.Should().BeNull();
    }
}
