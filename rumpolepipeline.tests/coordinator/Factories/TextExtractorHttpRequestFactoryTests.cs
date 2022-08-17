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

namespace rumpolepipeline.tests.coordinator.Factories
{
	public class TextExtractorHttpRequestFactoryTests
	{
        private readonly int _caseId;
		private readonly string _documentId;
		private readonly string _blobName;
		private readonly AccessToken _accessToken;
		private readonly string _content;
        private readonly string _textExtractorUrl;

		private readonly Mock<IDefaultAzureCredentialFactory> _mockDefaultAzureCredentialFactory;

        private readonly ITextExtractorHttpRequestFactory _textExtractorHttpRequestFactory;

		public TextExtractorHttpRequestFactoryTests()
		{
            var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_blobName = fixture.Create<string>();
			_accessToken = fixture.Create<AccessToken>();
			_content = fixture.Create<string>();
			var textExtractorScope = fixture.Create<string>();
			_textExtractorUrl = "https://www.test.co.uk/";

			_mockDefaultAzureCredentialFactory = new Mock<IDefaultAzureCredentialFactory>();
			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
			var mockDefaultAzureCredential = new Mock<DefaultAzureCredential>();

			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Returns(mockDefaultAzureCredential.Object);

			mockDefaultAzureCredential.Setup(credential => credential.GetTokenAsync(It.Is<TokenRequestContext>(trc => trc.Scopes.Single().Equals(textExtractorScope)), It.IsAny<CancellationToken>()))
				.ReturnsAsync(_accessToken);

			mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<TextExtractorRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.BlobName == _blobName)))
				.Returns(_content);

			mockConfiguration.Setup(config => config["TextExtractorScope"]).Returns(textExtractorScope);
			mockConfiguration.Setup(config => config["TextExtractorUrl"]).Returns(_textExtractorUrl);

			_textExtractorHttpRequestFactory = new TextExtractorHttpRequestFactory(_mockDefaultAzureCredentialFactory.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Uri.AbsoluteUri.Should().Be(_textExtractorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_accessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Throws(new Exception());

			await Assert.ThrowsAsync<TextExtractorHttpRequestFactoryException>(() => _textExtractorHttpRequestFactory.Create(_caseId, _documentId, _blobName));
		}
	}
}

