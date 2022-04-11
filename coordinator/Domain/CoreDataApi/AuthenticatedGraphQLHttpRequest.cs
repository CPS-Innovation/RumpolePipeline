using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace coordinator.Domain.CoreDataApi
{
    public class AuthenticatedGraphQLHttpRequest : GraphQLHttpRequest
    {
        private readonly string _accessToken;

        public AuthenticatedGraphQLHttpRequest(GraphQLHttpRequest request, string accessToken)
            : base(request)
        {
            _accessToken = accessToken;
        }

        public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
        {
            var message = base.ToHttpRequestMessage(options, serializer);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer",  _accessToken);
            message.Headers.Add("Correlation-Id", Guid.NewGuid().ToString());
            message.Headers.Add("Request-Ip-Address", "0.0.0.0");
            return message;
        }
    }
}
