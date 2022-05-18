using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using common.Wrappers;
using coordinator.Domain.Requests;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories
{
	public class GeneratePdfHttpRequestFactory : IGeneratePdfHttpRequestFactory
	{
        private readonly IDefaultAzureCredentialFactory _defaultAzureCredentialFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;

        public GeneratePdfHttpRequestFactory(
            IDefaultAzureCredentialFactory defaultAzureCredentialFactory,
            IJsonConvertWrapper jsonConvertWrapper,
            IConfiguration configuration)
		{
            _defaultAzureCredentialFactory = defaultAzureCredentialFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
           _configuration = configuration;
        }

        public async Task<DurableHttpRequest> Create(int caseId, string documentId, string fileName)
        {
            //TODO test
            var credential = _defaultAzureCredentialFactory.Create();
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext(new[] { _configuration["PdfGeneratorScope"] }));
            var headers = new Dictionary<string, StringValues>() {
                { "Content-Type", "application/json" },
                { "Authorization", $"Bearer {accessToken}"}
            };
            var content = _jsonConvertWrapper.SerializeObject(
                new GeneratePdfRequest { CaseId = caseId, DocumentId = documentId, FileName = fileName });

            return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration["PdfGeneratorUrl"]), headers, content);
        }
	}
}

