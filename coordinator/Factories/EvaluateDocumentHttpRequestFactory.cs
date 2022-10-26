using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Adapters;
using Common.Constants;
using Common.Domain.Requests;
using Common.Logging;
using Common.Wrappers;
using coordinator.Domain.Exceptions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories;

public class EvaluateDocumentHttpRequestFactory : IEvaluateDocumentHttpRequestFactory
{
    private readonly IIdentityClientAdapter _identityClientAdapter;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EvaluateDocumentHttpRequestFactory> _logger;

    public EvaluateDocumentHttpRequestFactory(IIdentityClientAdapter identityClientAdapter,
        IJsonConvertWrapper jsonConvertWrapper,
        IConfiguration configuration, ILogger<EvaluateDocumentHttpRequestFactory> logger)
    {
        _identityClientAdapter = identityClientAdapter ?? throw new ArgumentNullException(nameof(identityClientAdapter));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }
    
    public async Task<DurableHttpRequest> Create(int caseId, string documentId, string materialId, string lastUpdatedDate, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(Create), $"CaseId: {caseId}, DocumentId: {documentId}, MaterialId: {materialId}, LastUpdatedDate: {lastUpdatedDate}");
            
        try
        {
            var clientScopes = _configuration["PdfGeneratorScope"];
                
            var result = await _identityClientAdapter.GetClientAccessTokenAsync(clientScopes, correlationId);
                
            var headers = new Dictionary<string, StringValues>
            {
                { HttpHeaderKeys.ContentType, HttpHeaderValues.ApplicationJson },
                { HttpHeaderKeys.Authorization, $"{HttpHeaderValues.AuthTokenType} {result}"},
                { HttpHeaderKeys.CorrelationId, correlationId.ToString() }
            };
            var content = _jsonConvertWrapper.SerializeObject(
                new EvaluateDocumentRequest { CaseId = caseId, DocumentId = documentId, MaterialId = materialId, LastUpdatedDate = lastUpdatedDate });

            return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration["DocumentEvaluatorUrl"]), headers, content);
        }
        catch(Exception ex)
        {
            throw new GeneratePdfHttpRequestFactoryException(ex.Message);
        }
    }
}
