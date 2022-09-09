﻿using System;
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
	public class TextExtractorHttpRequestFactory : ITextExtractorHttpRequestFactory
    {
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IConfiguration _configuration;

        public TextExtractorHttpRequestFactory(IIdentityClientAdapter identityClientAdapter,
            IJsonConvertWrapper jsonConvertWrapper,
            IConfiguration configuration)
		{
            _identityClientAdapter = identityClientAdapter ?? throw new ArgumentNullException(nameof(identityClientAdapter));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<DurableHttpRequest> Create(int caseId, string documentId, string blobName, string currentAccessToken)
        {
            try
            {
                var clientScopes = _configuration["TextExtractorScopes"];
                
                var result = await _identityClientAdapter.GetAccessTokenOnBehalfOfAsync(currentAccessToken, clientScopes);
                
                var headers = new Dictionary<string, StringValues>() {
                    { "Content-Type", "application/json" },
                    { "Authorization", $"Bearer {result}"}
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

