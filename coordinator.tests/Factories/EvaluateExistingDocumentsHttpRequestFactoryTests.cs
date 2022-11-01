using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Common.Adapters;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Requests;
using Common.Wrappers;
using coordinator.Domain.Exceptions;
using coordinator.Factories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Factories
{
	public class EvaluateExistingDocumentsHttpRequestFactoryTests
	{
        private readonly int _caseId;
        private readonly List<CaseDocument> _existingCaseDocuments;
		private readonly AccessToken _clientAccessToken;
		private readonly string _content;
        private readonly string _existingDocumentsEvaluatorUrl;
        private readonly Guid _correlationId;

        private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;
        
		private readonly EvaluateExistingDocumentsHttpRequestFactory _evaluateExistingDocumentsHttpRequestFactory;

		public EvaluateExistingDocumentsHttpRequestFactoryTests()
		{
			var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_existingCaseDocuments = fixture.CreateMany<CaseDocument>(3).ToList();
			_clientAccessToken = fixture.Create<AccessToken>();
			_content = fixture.Create<string>();
			var pdfGeneratorScope = fixture.Create<string>();
			_existingDocumentsEvaluatorUrl = "https://www.test.co.uk/";
			_correlationId = fixture.Create<Guid>();

			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
            _mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();

            _mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
	            .ReturnsAsync(_clientAccessToken.Token);
			
			mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<EvaluateExistingDocumentsRequest>(r => r.CaseId == _caseId.ToString() && r.CaseDocuments == _existingCaseDocuments)))
				.Returns(_content);

			var mockLogger = new Mock<ILogger<EvaluateExistingDocumentsHttpRequestFactory>>();

			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.PdfGeneratorScope]).Returns(pdfGeneratorScope);
			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.ExistingDocumentsEvaluatorUrl]).Returns(_existingDocumentsEvaluatorUrl);
			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.OnBehalfOfTokenTenantId]).Returns(fixture.Create<string>());
			
			_evaluateExistingDocumentsHttpRequestFactory = new EvaluateExistingDocumentsHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _evaluateExistingDocumentsHttpRequestFactory.Create(_caseId, _existingCaseDocuments, _correlationId);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _evaluateExistingDocumentsHttpRequestFactory.Create(_caseId, _existingCaseDocuments, _correlationId);

			durableRequest.Uri.AbsoluteUri.Should().Be(_existingDocumentsEvaluatorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _evaluateExistingDocumentsHttpRequestFactory.Create(_caseId, _existingCaseDocuments, _correlationId);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_clientAccessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _evaluateExistingDocumentsHttpRequestFactory.Create(_caseId, _existingCaseDocuments, _correlationId);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ClientCredentialsFlow_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
				.Throws(new Exception());

			await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => _evaluateExistingDocumentsHttpRequestFactory.Create(_caseId, _existingCaseDocuments, _correlationId));
		}
	}
}

