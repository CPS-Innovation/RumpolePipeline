using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients;
using coordinator.Domain.CoreDataApi;
using coordinator.Domain.Exceptions;
using coordinator.Factories;
using FluentAssertions;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Clients
{
    public class CoreDataApiClientTests
    {
        private Fixture _fixture;
        private int _caseId;
        private string _accessToken;
        private AuthenticatedGraphQLHttpRequest _authenticatedGraphQLHttpRequest;
        private GraphQLResponse<GetCaseDetailsByIdResponse> _graphQLResponse;

        private Mock<IGraphQLClient> _mockGraphQLClient;
        private Mock<IAuthenticatedGraphQLHttpRequestFactory> _mockAuthenticatedGraphQLHttpRequestFactory;
        private Mock<ILogger<CoreDataApiClient>> _mockLogger;

        private ICoreDataApiClient CoreDataApiClient;

        public CoreDataApiClientTests()
        {
            _fixture = new Fixture();
            _caseId = _fixture.Create<int>();
            _accessToken = _fixture.Create<string>();
            _authenticatedGraphQLHttpRequest = _fixture.Create<AuthenticatedGraphQLHttpRequest>();
            _graphQLResponse = _fixture.Create<GraphQLResponse<GetCaseDetailsByIdResponse>>();

            _mockGraphQLClient = new Mock<IGraphQLClient>();
            _mockAuthenticatedGraphQLHttpRequestFactory = new Mock<IAuthenticatedGraphQLHttpRequestFactory>();
            _mockLogger = new Mock<ILogger<CoreDataApiClient>>();

            _mockAuthenticatedGraphQLHttpRequestFactory.Setup(factory => factory.Create(It.IsAny<GraphQLHttpRequest>(), _accessToken))
                .Returns(_authenticatedGraphQLHttpRequest);
            _mockGraphQLClient.Setup(client => client.SendQueryAsync<GetCaseDetailsByIdResponse>(_authenticatedGraphQLHttpRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_graphQLResponse);

            CoreDataApiClient = new CoreDataApiClient(_mockGraphQLClient.Object, _mockAuthenticatedGraphQLHttpRequestFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetCaseDocumentsByIdAsync_ReturnsCaseDetails()
        {
            var documents = await CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken);

            documents.Should().BeEquivalentTo(_graphQLResponse.Data.CaseDetails.Documents);
        }

        [Fact]
        public async Task GetCaseDocumentsByIdAsync_ReturnsEmptyListOfDocumentsWhenResponseIsNull()
        {
            _mockGraphQLClient.Setup(client => client.SendQueryAsync<GetCaseDetailsByIdResponse>(_authenticatedGraphQLHttpRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(default(GraphQLResponse<GetCaseDetailsByIdResponse>));
            var documents = await CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCaseDocumentsByIdAsync_ReturnsEmptyListOfDocumentsWhenResponseDataIsNull()
        {
            _graphQLResponse.Data = null;

            var documents = await CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCaseDocumentsByIdAsync_ReturnsEmptyListOfDocumentsWhenCaseDetailsIsNull()
        {
            _graphQLResponse.Data.CaseDetails = null;

            var documents = await CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCaseDocumentsByIdAsync_ReturnsEmptyListOfDocumentsWhenDocumentsIsEmpty()
        {
            _graphQLResponse.Data.CaseDetails.Documents = new List<Document>();

            var documents = await CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCaseDocumentssByIdAsync_ThrowsCoreDataApiExceptionWhenFailsToRetrieveCaseDetails()
        {
            _mockGraphQLClient.Setup(client => client.SendQueryAsync<GetCaseDetailsByIdResponse>(_authenticatedGraphQLHttpRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<CoreDataApiClientException>(() => CoreDataApiClient.GetCaseDocumentsByIdAsync(_caseId, _accessToken));
        }
    }
}
