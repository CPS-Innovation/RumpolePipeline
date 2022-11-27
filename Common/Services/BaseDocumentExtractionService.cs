using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Exceptions;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Common.Services;

public class BaseDocumentExtractionService
{
    private readonly ILogger _logger;
    private readonly IHttpRequestFactory _httpRequestFactory;
    private readonly HttpClient _httpClient;
    
    protected BaseDocumentExtractionService(ILogger logger, IHttpRequestFactory httpRequestFactory, HttpClient httpClient)
    {
        _logger = logger;
        _httpRequestFactory = httpRequestFactory;
        _httpClient = httpClient;
    }
    
    protected async Task<HttpContent> GetHttpContentAsync(string requestUri, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetHttpContentAsync), $"RequestUri: {requestUri}");
            
        var request = _httpRequestFactory.CreateGet(requestUri, accessToken, correlationId);
        var response = await _httpClient.SendAsync(request);

        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException exception)
        {
            throw new HttpException(response.StatusCode, exception);
        }

        var result = response.Content;
        _logger.LogMethodExit(correlationId, nameof(GetHttpContentAsync), string.Empty);
        return result;
    }
}
