using GraphQL.Client.Http;
using coordinator.Domain.CoreDataApi;

namespace coordinator.Factories
{
    public class AuthenticatedGraphQLHttpRequestFactory : IAuthenticatedGraphQLHttpRequestFactory
    {
        public AuthenticatedGraphQLHttpRequest Create(GraphQLHttpRequest graphQLHttpRequest, string accessToken)
        {
            return new AuthenticatedGraphQLHttpRequest(graphQLHttpRequest, accessToken);
        }
    }
}
