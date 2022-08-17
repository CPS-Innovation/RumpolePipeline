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
	public class GeneratePdfHttpRequestFactoryTests
	{
        private readonly int _caseId;
		private readonly string _documentId;
		private readonly string _fileName;
		private readonly AccessToken _accessToken;
		private readonly string _content;
        private readonly string _pdfGeneratorUrl;

		private readonly Mock<IDefaultAzureCredentialFactory> _mockDefaultAzureCredentialFactory;

        private readonly GeneratePdfHttpRequestFactory _generatePdfHttpRequestFactory;

		public GeneratePdfHttpRequestFactoryTests()
		{
            var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_documentId = fixture.Create<string>();
			_fileName = fixture.Create<string>();
			_accessToken = fixture.Create<AccessToken>();
			_content = fixture.Create<string>();
			var pdfGeneratorScope = fixture.Create<string>();
			_pdfGeneratorUrl = "https://www.test.co.uk/";

			_mockDefaultAzureCredentialFactory = new Mock<IDefaultAzureCredentialFactory>();
			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
			var mockDefaultAzureCredential = new Mock<DefaultAzureCredential>();

			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Returns(mockDefaultAzureCredential.Object);

			mockDefaultAzureCredential.Setup(credential => credential.GetTokenAsync(It.Is<TokenRequestContext>(trc => trc.Scopes.Single().Equals(pdfGeneratorScope)), It.IsAny<CancellationToken>()))
				.ReturnsAsync(_accessToken);

			mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.FileName == _fileName)))
				.Returns(_content);

			mockConfiguration.Setup(config => config["PdfGeneratorScope"]).Returns(pdfGeneratorScope);
			mockConfiguration.Setup(config => config["PdfGeneratorUrl"]).Returns(_pdfGeneratorUrl);

			_generatePdfHttpRequestFactory = new GeneratePdfHttpRequestFactory(_mockDefaultAzureCredentialFactory.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Uri.AbsoluteUri.Should().Be(_pdfGeneratorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_accessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Throws(new Exception());

			await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName));
		}
	}
}

