using coordinator.Domain.CoreDataApi;
using coordinator.Domain.Exceptions;
using coordinator.Factories;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public class CoreDataApiClient : ICoreDataApiClient
    {
        private readonly IGraphQLClient _graphQLClient;
        private readonly IAuthenticatedGraphQLHttpRequestFactory _authenticatedGraphQLHttpRequestFactory;
        public CoreDataApiClient(IGraphQLClient graphQLClient,
            IAuthenticatedGraphQLHttpRequestFactory authenticatedGraphQLHttpRequestFactory)
        {
            _graphQLClient = graphQLClient;
            _authenticatedGraphQLHttpRequestFactory = authenticatedGraphQLHttpRequestFactory;
        }
         
        public async Task<CaseDetails> GetCaseDetailsById(int caseId, string accessToken)
        {
            try
            {
                var query = new GraphQLHttpRequest
                {
                    Query = "query {case(id: " + caseId + ")  {id documents { id type { code name } }  }}"
                };

                var authenticatedRequest = _authenticatedGraphQLHttpRequestFactory.Create(query, accessToken);
                var response = await _graphQLClient.SendQueryAsync<GetCaseDetailsByIdResponse>(authenticatedRequest);

                if (response?.Data?.CaseDetails == null)
                {
                    //TODO return this? or log warning no details found?
                    return null;
                }

                return response.Data.CaseDetails;
            }
            catch (Exception exception)
            {
                throw new CoreDataApiClientException($"Failed to retrieve case details by id for case id '{caseId}'.", innerException: exception);
            }
        }
    }
}
