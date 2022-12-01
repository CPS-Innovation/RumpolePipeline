using AutoFixture;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Common.Services.SearchService;
using Common.Services.SearchService.Contracts;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.tests.Services.SearchService;

public class SearchServiceProcessorTests
{
    private readonly Fixture _fixture;
    private readonly Guid _correlationId;
    private readonly Mock<SearchClient> _mockSearchClient;
    
    private readonly SearchOptions _searchOptionsByCaseId;
    private readonly SearchOptions _searchOptionsByCaseAndDocumentId;
    
    private readonly ISearchServiceProcessor _searchServiceProcessor;

    public SearchServiceProcessorTests()
    {
        _fixture = new Fixture();
        var caseId = _fixture.Create<string>();
        var documentId = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();

        _searchOptionsByCaseId = new SearchOptions
        {
            Filter = $"caseId eq {caseId}"
        };
        _searchOptionsByCaseId.OrderBy.Add("id");
        
        _searchOptionsByCaseAndDocumentId = new SearchOptions
        {
            Filter = $"caseId eq {caseId} and documentId eq '{documentId}'"
        };
        _searchOptionsByCaseId.OrderBy.Add("id");
        
        _mockSearchClient = new Mock<SearchClient>();
        var mockResponse = new Mock<Response<SearchResults<SearchLine>>>();
        var mockSearchResults = new Mock<SearchResults<SearchLine>>();
        
        var mockSearchClientFactory = new Mock<ISearchClientFactory>();
        
        mockSearchClientFactory.Setup(factory => factory.Create()).Returns(_mockSearchClient.Object);
        _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>("*", It.Is<SearchOptions>(o => o.Filter == _searchOptionsByCaseId.Filter), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);
        _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>("*", It.Is<SearchOptions>(o => o.Filter == _searchOptionsByCaseAndDocumentId.Filter), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);
        mockResponse.Setup(response => response.Value).Returns(mockSearchResults.Object);

        var loggerMock = new Mock<ILogger<SearchServiceProcessor>>();
        
        _searchServiceProcessor = new SearchServiceProcessor(loggerMock.Object, mockSearchClientFactory.Object);
    }
    
    [Fact]
    public async Task SearchForDocumentsAsync_ByCaseId_ReturnsSearchLines()
    {
        var results = await _searchServiceProcessor.SearchForDocumentsAsync(_searchOptionsByCaseId, _correlationId);

        results.Should().NotBeNull();
    }
    
    [Fact]
    public async Task SearchForDocumentsAsync_ByCaseAndDocumentId_ReturnsSearchLines()
    {
        var results = await _searchServiceProcessor.SearchForDocumentsAsync(_searchOptionsByCaseAndDocumentId, _correlationId);

        results.Should().NotBeNull();
    }
    
    [Fact]
    public async Task SearchForDocumentsAsync_ByCaseId_ResultsAreOrderedByDocumentId()
    {
        var responseMock = new Mock<Response>();
        var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
        fakeSearchLines[0].DocumentId = "XYZ";
        fakeSearchLines[1].DocumentId = "LMN";
        fakeSearchLines[2].DocumentId = "ABC";
			
        _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>("*", 
                It.Is<SearchOptions>(o => o.Filter == _searchOptionsByCaseId.Filter), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                Response.FromValue(
                    SearchModelFactory.SearchResults(new[] {
                        SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
                        SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
                        SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
                    }, 100, null, null, responseMock.Object), responseMock.Object)));
			
        var results = await _searchServiceProcessor.SearchForDocumentsAsync(_searchOptionsByCaseId, _correlationId);

        using (new AssertionScope())
        {
            results.Count.Should().Be(3);
            results[0].DocumentId.Should().Be("ABC");
            results[1].DocumentId.Should().Be("LMN");
            results[2].DocumentId.Should().Be("XYZ");
        }
    }
    
    [Fact]
    public async Task SearchForDocumentsAsync_ByCaseAndDocumentId_ResultsAreOrderedByDocumentId()
    {
        var responseMock = new Mock<Response>();
        var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
        fakeSearchLines[0].DocumentId = "XYZ";
        fakeSearchLines[1].DocumentId = "LMN";
        fakeSearchLines[2].DocumentId = "ABC";
			
        _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>("*", 
                It.Is<SearchOptions>(o => o.Filter == _searchOptionsByCaseAndDocumentId.Filter), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                Response.FromValue(
                    SearchModelFactory.SearchResults(new[] {
                        SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
                        SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
                        SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
                    }, 100, null, null, responseMock.Object), responseMock.Object)));
			
        var results = await _searchServiceProcessor.SearchForDocumentsAsync(_searchOptionsByCaseAndDocumentId, _correlationId);

        using (new AssertionScope())
        {
            results.Count.Should().Be(3);
            results[0].DocumentId.Should().Be("ABC");
            results[1].DocumentId.Should().Be("LMN");
            results[2].DocumentId.Should().Be("XYZ");
        }
    }
}
