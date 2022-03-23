using System.Threading.Tasks;
using coordinator.Domain.CoreDataApi;
using GraphQL.Client.Http;

namespace coordinator.Factories
{
    public interface IAuthenticatedGraphQLHttpRequestFactory
    {
        Task<AuthenticatedGraphQLHttpRequest> Create(GraphQLHttpRequest graphQLHttpRequest, string accessToken);
    }
}
