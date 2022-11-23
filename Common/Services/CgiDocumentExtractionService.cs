using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Services;

public class CgiDocumentExtractionService : BaseDocumentExtractionService
{
    private readonly ILogger<CgiDocumentExtractionService> _logger;
    private readonly IConfiguration _configuration;
    
    public CgiDocumentExtractionService(HttpClient httpClient, IHttpRequestFactory httpRequestFactory, ILogger<CgiDocumentExtractionService> logger, IConfiguration configuration)
        : base(logger, httpRequestFactory, httpClient)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override async Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"DocumentId: {documentId}, FileName: {fileName}");
        //It has been assumed here that CDE will return 404 not found when document cant be found. Test this when hooked up properly
        var content = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.GetDocumentUrl], documentId, fileName), accessToken, correlationId);
        var result = await content.ReadAsStreamAsync();
        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        return result;
    }
}