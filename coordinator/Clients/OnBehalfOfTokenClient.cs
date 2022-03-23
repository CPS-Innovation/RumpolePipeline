﻿using coordinator.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace coordinator.Clients
{
    class OnBehalfOfTokenClient : IOnBehalfOfTokenClient
    {
        private readonly IConfidentialClientApplication _application;
        private readonly IConfiguration _configuration;

        private const string assertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        public OnBehalfOfTokenClient(IConfidentialClientApplication application, 
                                     IConfiguration configuration)
        {
            _application = application;
            _configuration = configuration;
        }

        public async Task<string> GetAccessToken(string accessToken)
        {
            AuthenticationResult result;
            
            try
            {
                var userAssertion = new UserAssertion(accessToken, assertionType);
                //TODO add configuration scope to terraform "api://5f1f433a-41b3-45d3-895e-927f50232a47/case.confirm"
                var scopes = new Collection<string> { _configuration["CoreDataApiScope"] };
                result = await _application.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
            }
            catch (MsalException exception)
            {
                throw new OnBehalfOfTokenClientException("Failed to acquire onBehalfOf token.", exception);
            }

            return result.AccessToken;
        }
    }
}
