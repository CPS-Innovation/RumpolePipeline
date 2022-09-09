using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using common.Wrappers;
using coordinator.Domain.Adapters;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Requests;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories
{
	public class GeneratePdfHttpRequestFactory : IGeneratePdfHttpRequestFactory
	{
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;

        public GeneratePdfHttpRequestFactory(IIdentityClientAdapter identityClientAdapter,
            IJsonConvertWrapper jsonConvertWrapper,
            IConfiguration configuration)
		{
            _identityClientAdapter = identityClientAdapter ?? throw new ArgumentNullException(nameof(identityClientAdapter));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DurableHttpRequest> Create(int caseId, string documentId, string fileName, string currentAccessToken)
        {
            try
            {
                var clientScopes = _configuration["PdfGeneratorScope"];
                
                var result = await _identityClientAdapter.GetAccessTokenOnBehalfOfAsync(currentAccessToken, clientScopes);
                
                var headers = new Dictionary<string, StringValues>() {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {result}"}
                };
                var content = _jsonConvertWrapper.SerializeObject(
                    new GeneratePdfRequest { CaseId = caseId, DocumentId = documentId, FileName = fileName });

                return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration["PdfGeneratorUrl"]), headers, content);
            }
            catch(Exception ex)
            {
                throw new GeneratePdfHttpRequestFactoryException(ex.Message);
            }
        }
	}
}

