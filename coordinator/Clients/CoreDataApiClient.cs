using coordinator.Domain.CoreDataApi;
using coordinator.Domain.Exceptions;
using coordinator.Factories;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    public class CoreDataApiClient : ICoreDataApiClient
    {
        private readonly IGraphQLClient _graphQLClient;
        private readonly IAuthenticatedGraphQLHttpRequestFactory _authenticatedGraphQLHttpRequestFactory;
        private readonly ILogger<CoreDataApiClient> _log;

        public CoreDataApiClient(IGraphQLClient graphQLClient,
            IAuthenticatedGraphQLHttpRequestFactory authenticatedGraphQLHttpRequestFactory,
            ILogger<CoreDataApiClient> log)
        {
            _graphQLClient = graphQLClient;
            _authenticatedGraphQLHttpRequestFactory = authenticatedGraphQLHttpRequestFactory;
            _log = log;
        }
         
        public async Task<List<Document>> GetCaseDocumentsByIdAsync(int caseId, string accessToken)
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
                    _log.LogInformation($"No data found for case with id '{caseId}'.");
                    return new List<Document>();
                }

                var documents = response.Data.CaseDetails.Documents;
                if(documents.Count == 0)
                {
                    _log.LogInformation($"No documents found for case id '{caseId}'.");
                }

                return documents;
            }
            catch (Exception exception)
            {
                throw new CoreDataApiClientException($"Failed to retrieve case details for case id '{caseId}'. Exception: {exception.Message}.");
            }
        }
    }
}
