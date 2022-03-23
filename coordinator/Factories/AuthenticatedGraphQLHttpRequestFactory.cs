using GraphQL.Client.Http;
using coordinator.Domain.CoreDataApi;
using System.Threading.Tasks;

namespace coordinator.Factories
{
    public class AuthenticatedGraphQLHttpRequestFactory : IAuthenticatedGraphQLHttpRequestFactory
    {
        public async Task<AuthenticatedGraphQLHttpRequest> Create(GraphQLHttpRequest graphQLHttpRequest, string accessToken)
        {
            return new AuthenticatedGraphQLHttpRequest(graphQLHttpRequest, accessToken);
        }
    }
}
