using coordinator.Domain.CoreDataApi;
using coordinator.Factories;
using FluentAssertions;
using GraphQL.Client.Http;
using Xunit;

namespace coordinator.tests.Factories
{
    public class AuthenticatedGraphQLHttpRequestFactoryTests
    {
        [Fact]
        public void Create_CreatesAuthenticatedRequest()
        {
            var factory = new AuthenticatedGraphQLHttpRequestFactory();

            var authenticatedRequest = factory.Create(new GraphQLHttpRequest(), "accessToken");

            authenticatedRequest.Should().BeOfType<AuthenticatedGraphQLHttpRequest>();
        }
    }
}
