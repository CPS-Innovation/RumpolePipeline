using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using common.Wrappers;
using coordinator.Domain.Adapters;
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
        private readonly int _caseId;
		private readonly string _documentId;
		private readonly string _fileName;
		private readonly AccessToken _accessToken;
		private readonly string _content;
        private readonly string _pdfGeneratorUrl;

        private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;
        

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

			var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
			var mockConfiguration = new Mock<IConfiguration>();
            _mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();

            _mockIdentityClientAdapter.Setup(x => x.GetAccessTokenOnBehalfOfAsync(It.IsAny<string>(), It.IsAny<string>()))
	            .ReturnsAsync(_accessToken.Token);
			
			mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.Is<GeneratePdfRequest>(r => r.CaseId == _caseId && r.DocumentId == _documentId && r.FileName == _fileName)))
				.Returns(_content);

			mockConfiguration.Setup(config => config["PdfGeneratorScopes"]).Returns(pdfGeneratorScope);
			mockConfiguration.Setup(config => config["PdfGeneratorUrl"]).Returns(_pdfGeneratorUrl);
			mockConfiguration.Setup(config => config["OnBehalfOfTokenTenantId"]).Returns(fixture.Create<string>());
			mockConfiguration.Setup(config => config["PdfGeneratorClientId"]).Returns(fixture.Create<string>());
			mockConfiguration.Setup(config => config["PdfGeneratorClientSecret"]).Returns(fixture.Create<string>());

			_generatePdfHttpRequestFactory = new GeneratePdfHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object);
		}

		[Fact]
		public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName, _accessToken.Token);

			durableRequest.Method.Should().Be(HttpMethod.Post);
		}

		[Fact]
		public async Task Create_SetsExpectedUriOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName, _accessToken.Token);

			durableRequest.Uri.AbsoluteUri.Should().Be(_pdfGeneratorUrl);
		}

		[Fact]
		public async Task Create_SetsExpectedHeadersOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName, _accessToken.Token);

			durableRequest.Headers.Should().Contain("Content-Type", "application/json");
			durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_accessToken.Token}");
		}

		[Fact]
		public async Task Create_SetsExpectedContentOnDurableRequest()
		{
			var durableRequest = await _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName, _accessToken.Token);

			durableRequest.Content.Should().Be(_content);
		}

		[Fact]
		public async Task Create_ThrowsExceptionWhenExceptionOccurs()
		{
			_mockIdentityClientAdapter.Setup(x => x.GetAccessTokenOnBehalfOfAsync(It.IsAny<string>(), It.IsAny<string>()))
				.Throws(new Exception());

			await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => _generatePdfHttpRequestFactory.Create(_caseId, _documentId, _fileName, _accessToken.Token));
		}
	}
}

