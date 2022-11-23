using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Responses;
using Common.Factories.Contracts;
using Common.Logging;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Common.Services;

public class DdeiDocumentExtractionService : BaseDocumentExtractionService
{
    private readonly ILogger<DdeiDocumentExtractionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly ICaseDocumentMapper<DdeiCaseDocumentResponse> _caseDocumentMapper;

    public DdeiDocumentExtractionService(HttpClient httpClient, IHttpRequestFactory httpRequestFactory, ILogger<DdeiDocumentExtractionService> logger, 
        IConfiguration configuration, IJsonConvertWrapper jsonConvertWrapper, ICaseDocumentMapper<DdeiCaseDocumentResponse> caseDocumentMapper)
        : base(logger, httpRequestFactory, httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _jsonConvertWrapper = jsonConvertWrapper;
        _caseDocumentMapper = caseDocumentMapper;
    }

    public override async Task<Stream> GetDocumentAsync(string caseUrn, string caseId, string documentId, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}, DocumentId: {documentId}");
        
        var content = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.GetDocumentUrl], caseUrn, caseId, documentId), accessToken, correlationId);
        var result = await content.ReadAsStreamAsync();
        
        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        return result;
    }

    public override async Task<CaseDocument[]> ListDocumentsAsync(string caseUrn, string caseId, string accessToken, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(GetDocumentAsync), $"CaseUrn: {caseUrn}, CaseId: {caseId}");

        var response = await GetHttpContentAsync(string.Format(_configuration[ConfigKeys.SharedKeys.ListDocumentsUrl], caseUrn, caseId), accessToken, correlationId);
        var stringContent = await response.ReadAsStringAsync();
        var ddeiResults = _jsonConvertWrapper.DeserializeObject<List<DdeiCaseDocumentResponse>>(stringContent);

        _logger.LogMethodExit(correlationId, nameof(GetDocumentAsync), string.Empty);
        return ddeiResults.Select(ddeiResult => _caseDocumentMapper.Map(ddeiResult)).Where(mappedResult => mappedResult != null).ToArray();
    }
}
