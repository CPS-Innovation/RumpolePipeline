using coordinator.Domain.CoreDataApi;
using GraphQL.Client.Http;

namespace coordinator.Factories
{
    public interface IAuthenticatedGraphQLHttpRequestFactory
    {
        AuthenticatedGraphQLHttpRequest Create(GraphQLHttpRequest graphQLHttpRequest, string accessToken);
    }
}
