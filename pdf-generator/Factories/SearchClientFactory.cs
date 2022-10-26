using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;

namespace pdf_generator.Factories;

public class SearchClientFactory : ISearchClientFactory
{
    private readonly IConfiguration _configuration;
    
    public SearchClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SearchClient Create()
    {
        var sc = new SearchClient(
            new Uri(_configuration["SearchClientEndpointUrl"]),
            _configuration["SearchClientIndexName"],
            new AzureKeyCredential(_configuration["SearchClientAuthorizationKey"]),
            new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
        
        return sc;
    }
}