using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Azure.Identity;
using common.Wrappers;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Requests;
using coordinator.Factories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace coordinator.tests.Factories
{
	public class TextExtractorHttpRequestFactoryTests
	{
		private Fixture _fixture;
		private int _caseId;
		private string _documentId;
		private string _blobName;
		private AccessToken _accessToken;
		private string _content;
		private string _textExtractorScope;
		private string _textExtractorUrl;

		private Mock<IDefaultAzureCredentialFactory> _mockDefaultAzureCredentialFactory;
		private Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
		private Mock<IConfiguration> _mockConfiguration;
		private Mock<DefaultAzureCredential> _mockDefaultAzureCredential;

		private ITextExtractorHttpRequestFactory TextExtractorHttpRequestFactory;

		public TextExtractorHttpRequestFactoryTests()
		{
			_fixture = new Fixture();
			_caseId = _fixture.Create<int>();
			_documentId = _fixture.Create<string>();
			_blobName = _fixture.Create<string>();
			_accessToken = _fixture.Create<AccessToken>();
			_content = _fixture.Create<string>();
			_textExtractorScope = _fixture.Create<string>();
			_textExtractorUrl = "https://www.test.co.uk/";

			_mockDefaultAzureCredentialFactory = new Mock<IDefaultAzureCredentialFactory>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockConfiguration = new Mock<IConfiguration>();
			_mockDefaultAzureCredential = new Mock<DefaultAzureCredential>();

			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Returns(_mockDefaultAzureCredential.Object);

			_mockDefaultAzureCredential.Setup(credential => credential.GetTokenAsync(It.Is<TokenRequestContext>(trc => trc.Scopes.Single().Equals(_textExtractorScope)), It.IsAny<CancellationToken>()))
				.ReturnsAsync(_accessToken);

			_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<TextExtractorRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.BlobName == _blobName)))
				.Returns(_content);

			_mockConfiguration.Setup(config => config["TextExtractorScope"]).Returns(_textExtractorScope);
			_mockConfiguration.Setup(config => config["TextExtractorUrl"]).Returns(_textExtractorUrl);

			TextExtractorHttpRequestFactory = new TextExtractorHttpRequestFactory(_mockDefaultAzureCredentialFactory.Object, _mockJsonConvertWrapper.Object, _mockConfiguration.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await TextExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await TextExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Uri.AbsoluteUri.Should().Be(_textExtractorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await TextExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_accessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await TextExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Throws(new Exception());

			await Assert.ThrowsAsync<TextExtractorHttpRequestFactoryException>(() => TextExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName));
		}
	}
}

