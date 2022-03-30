using System;
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
        private GetCaseDetailsByIdResponse _getCaseDetailsByIdResponse;

        private Mock<IGraphQLClient> _mockGraphQLClient;
        private Mock<IAuthenticatedGraphQLHttpRequestFactory> _mockAuthenticatedGraphQLHttpRequestFactory;

        private ICoreDataApiClient CoreDataApiClient;

        public CoreDataApiClientTests()
        {
            _fixture = new Fixture();
            _caseId = _fixture.Create<int>();
            _accessToken = _fixture.Create<string>();
            _authenticatedGraphQLHttpRequest = _fixture.Create<AuthenticatedGraphQLHttpRequest>();
            _getCaseDetailsByIdResponse = _fixture.Create<GetCaseDetailsByIdResponse>();
            _graphQLResponse = _fixture.Create<GraphQLResponse<GetCaseDetailsByIdResponse>>();

            _mockGraphQLClient = new Mock<IGraphQLClient>();
            _mockAuthenticatedGraphQLHttpRequestFactory = new Mock<IAuthenticatedGraphQLHttpRequestFactory>();

            _mockAuthenticatedGraphQLHttpRequestFactory.Setup(factory => factory.Create(It.IsAny<GraphQLHttpRequest>(), _accessToken))
                .Returns(_authenticatedGraphQLHttpRequest);
            _mockGraphQLClient.Setup(client => client.SendQueryAsync<GetCaseDetailsByIdResponse>(_authenticatedGraphQLHttpRequest, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_graphQLResponse);

            CoreDataApiClient = new CoreDataApiClient(_mockGraphQLClient.Object, _mockAuthenticatedGraphQLHttpRequestFactory.Object);
        }

        [Fact]
        public async Task GetCaseDetailsByIdAsync_ReturnsCaseDetails()
        {
            var caseDetails = await CoreDataApiClient.GetCaseDetailsByIdAsync(_caseId, _accessToken);

            caseDetails.Should().Be(_graphQLResponse.Data.CaseDetails);
        }

        [Fact]
        public async Task GetCaseDetailsByIdAsync_ThrowsCoreDataApiExceptionWhenFailsToRetrieveCaseDetails()
        {
            _mockGraphQLClient.Setup(client => client.SendQueryAsync<GetCaseDetailsByIdResponse>(_authenticatedGraphQLHttpRequest, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<CoreDataApiClientException>(() => CoreDataApiClient.GetCaseDetailsByIdAsync(_caseId, _accessToken));
        }
    }
}
