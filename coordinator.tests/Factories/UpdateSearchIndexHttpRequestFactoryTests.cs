using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Common.Adapters;
using Common.Constants;
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
	public class UpdateSearchIndexHttpRequestFactoryTests
	{
        private readonly long _caseId;
		private readonly string _documentId;
		private readonly AccessToken _clientAccessToken;
		private readonly string _content;
        private readonly string _searchIndexUpdateUrl;
        private readonly Guid _correlationId;

        private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;
        
		private readonly UpdateSearchIndexHttpRequestFactory _updateSearchIndexHttpRequestFactory;

		public UpdateSearchIndexHttpRequestFactoryTests()
		{
			var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_clientAccessToken = fixture.Create<AccessToken>();
			_content = fixture.Create<string>();
			var textExtractorScope = fixture.Create<string>();
			_searchIndexUpdateUrl = "https://www.test.co.uk/";
			_correlationId = fixture.Create<Guid>();

			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
            _mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();

            _mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
	            .ReturnsAsync(_clientAccessToken.Token);
			
			mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<UpdateSearchIndexRequest>(r => r.CaseId == _caseId.ToString() && r.DocumentId == _documentId)))
				.Returns(_content);

			var mockLogger = new Mock<ILogger<UpdateSearchIndexHttpRequestFactory>>();

			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.TextExtractorScope]).Returns(textExtractorScope);
			mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.SearchIndexUpdateUrl]).Returns(_searchIndexUpdateUrl);
			
			_updateSearchIndexHttpRequestFactory = new UpdateSearchIndexHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _updateSearchIndexHttpRequestFactory.Create(_caseId, _documentId, _correlationId);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _updateSearchIndexHttpRequestFactory.Create(_caseId, _documentId, _correlationId);

			durableRequest.Uri.AbsoluteUri.Should().Be(_searchIndexUpdateUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _updateSearchIndexHttpRequestFactory.Create(_caseId, _documentId, _correlationId);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_clientAccessToken.Token}");
			durableRequest.Headers.Should().Contain("Correlation-Id", _correlationId.ToString());
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _updateSearchIndexHttpRequestFactory.Create(_caseId, _documentId, _correlationId);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ClientCredentialsFlow_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
				.Throws(new Exception());

			await Assert.ThrowsAsync<TextExtractorHttpRequestFactoryException>(() => _updateSearchIndexHttpRequestFactory.Create(_caseId, _documentId, _correlationId));
		}
	}
}

