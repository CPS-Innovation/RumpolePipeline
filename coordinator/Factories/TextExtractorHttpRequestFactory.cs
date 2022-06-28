using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core;
using common.Wrappers;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Requests;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace coordinator.Factories
{
	public class TextExtractorHttpRequestFactory : ITextExtractorHttpRequestFactory
    {
        private readonly IDefaultAzureCredentialFactory _defaultAzureCredentialFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;

        public TextExtractorHttpRequestFactory(
            IDefaultAzureCredentialFactory defaultAzureCredentialFactory,
            IJsonConvertWrapper jsonConvertWrapper,
            IConfiguration configuration)
		{
            _defaultAzureCredentialFactory = defaultAzureCredentialFactory;
            _jsonConvertWrapper = jsonConvertWrapper;
           _configuration = configuration;
        }

        public async Task<DurableHttpRequest> Create(int caseId, string documentId, string blobName)
        {
            try
            {
                var credential = _defaultAzureCredentialFactory.Create();
                var accessToken = await credential.GetTokenAsync(new TokenRequestContext(new[] { _configuration["TextExtractorScope"] }));
                var headers = new Dictionary<string, StringValues>() {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {accessToken.Token}"}
                };
                var content = _jsonConvertWrapper.SerializeObject(
                    new TextExtractorRequest { CaseId = caseId, DocumentId = documentId, BlobName = blobName });

                return new DurableHttpRequest(HttpMethod.Post, new Uri(_configuration["TextExtractorUrl"]), headers, content);
            }
            catch(Exception ex)
            {
                throw new TextExtractorHttpRequestFactoryException(ex.Message);
            }
        }
	}
}

