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
	public class GeneratePdfHttpRequestFactoryTests
	{
		private Fixture _fixture;
		private int _caseId;
		private string _documentId;
		private string _fileName;
		private AccessToken _accessToken;
		private string _content;
		private string _pdfGeneratorScope;
		private string _pdfGeneratorUrl;

		private Mock<IDefaultAzureCredentialFactory> _mockDefaultAzureCredentialFactory;
		private Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
		private Mock<IConfiguration> _mockConfiguration;
		private Mock<DefaultAzureCredential>_mockDefaultAzureCredential;

		private GeneratePdfHttpRequestFactory GeneratePdfHttpRequestFactory;

		public GeneratePdfHttpRequestFactoryTests()
		{
			_fixture = new Fixture();
			_caseId = _fixture.Create<int>();
			_documentId = _fixture.Create<string>();
			_fileName = _fixture.Create<string>();
			_accessToken = _fixture.Create<AccessToken>();
			_content = _fixture.Create<string>();
			_pdfGeneratorScope = _fixture.Create<string>();
			_pdfGeneratorUrl = "https://www.test.co.uk/";

			_mockDefaultAzureCredentialFactory = new Mock<IDefaultAzureCredentialFactory>();
			_mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			_mockConfiguration = new Mock<IConfiguration>();
			_mockDefaultAzureCredential = new Mock<DefaultAzureCredential>();

			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Returns(_mockDefaultAzureCredential.Object);

			_mockDefaultAzureCredential.Setup(credential => credential.GetTokenAsync(It.Is<TokenRequestContext>(trc => trc.Scopes.Single().Equals(_pdfGeneratorScope)), It.IsAny<CancellationToken>()))
				.ReturnsAsync(_accessToken);

			_mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.FileName == _fileName)))
				.Returns(_content);

			_mockConfiguration.Setup(config => config["PdfGeneratorScope"]).Returns(_pdfGeneratorScope);
			_mockConfiguration.Setup(config => config["PdfGeneratorUrl"]).Returns(_pdfGeneratorUrl);

			GeneratePdfHttpRequestFactory = new GeneratePdfHttpRequestFactory(_mockDefaultAzureCredentialFactory.Object, _mockJsonConvertWrapper.Object, _mockConfiguration.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
        {
			var durableRequest = await GeneratePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Method.Should().Be(HttpMethod.Post);
        }

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await GeneratePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Uri.AbsoluteUri.Should().Be(_pdfGeneratorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await GeneratePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_accessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await GeneratePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockDefaultAzureCredentialFactory.Setup(factory => factory.Create()).Throws(new Exception());

			await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => GeneratePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName));
		}
	}
}

