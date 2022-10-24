using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Core;
using Common.Adapters;
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
	public class EvaluateDocumentHttpRequestFactoryTests
	{
	private readonly int _caseId;
	private readonly string _documentId;
	private readonly string _materialId;
	private readonly string _lastUpdatedDate;
	private readonly AccessToken _clientAccessToken;
	private readonly string _content;
	private readonly string _documentEvaluatorUrl;
	private readonly Guid _correlationId;

	private readonly Mock<IIdentityClientAdapter> _mockIdentityClientAdapter;

	private readonly EvaluateDocumentHttpRequestFactory _evaluateDocumentHttpRequestFactory;

	public EvaluateDocumentHttpRequestFactoryTests()
	{
		var fixture = new Fixture();
		_caseId = fixture.Create<int>();
		_documentId = fixture.Create<string>();
		_materialId = fixture.Create<string>();
		_lastUpdatedDate = fixture.Create<string>();
		_clientAccessToken = fixture.Create<AccessToken>();
		_content = fixture.Create<string>();
		var pdfGeneratorScope = fixture.Create<string>();
		_documentEvaluatorUrl = "https://www.test.co.uk/";
		_correlationId = fixture.Create<Guid>();

		var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
		var mockConfiguration = new Mock<IConfiguration>();
		_mockIdentityClientAdapter = new Mock<IIdentityClientAdapter>();

		_mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
			.ReturnsAsync(_clientAccessToken.Token);

		mockJsonConvertWrapper.Setup(wrapper =>
				wrapper.SerializeObject(It.Is<EvaluateDocumentRequest>(r => r.CaseId == _caseId && 
				                                                            r.DocumentId == _documentId && r.MaterialId == _materialId && r.LastUpdatedDate == _lastUpdatedDate)))
			.Returns(_content);

		var mockLogger = new Mock<ILogger<EvaluateDocumentHttpRequestFactory>>();

		mockConfiguration.Setup(config => config["PdfGeneratorScope"]).Returns(pdfGeneratorScope);
		mockConfiguration.Setup(config => config["DocumentEvaluatorUrl"]).Returns(_documentEvaluatorUrl);
		mockConfiguration.Setup(config => config["OnBehalfOfTokenTenantId"]).Returns(fixture.Create<string>());

		_evaluateDocumentHttpRequestFactory =
			new EvaluateDocumentHttpRequestFactory(_mockIdentityClientAdapter.Object, mockJsonConvertWrapper.Object, mockConfiguration.Object, mockLogger.Object);
	}

	[Fact]
	public async Task Create_SetsExpectedHttpMethodOnDurableRequest()
	{
		var durableRequest = await _evaluateDocumentHttpRequestFactory.Create(_caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId);

		durableRequest.Method.Should().Be(HttpMethod.Post);
	}

	[Fact]
	public async Task Create_SetsExpectedUriOnDurableRequest()
	{
		var durableRequest = await _evaluateDocumentHttpRequestFactory.Create(_caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId);

		durableRequest.Uri.AbsoluteUri.Should().Be(_documentEvaluatorUrl);
	}

	[Fact]
	public async Task Create_SetsExpectedHeadersOnDurableRequest()
	{
		var durableRequest = await _evaluateDocumentHttpRequestFactory.Create(_caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId);

		durableRequest.Headers.Should().Contain("Content-Type", "application/json");
		durableRequest.Headers.Should().Contain("Authorization", $"Bearer {_clientAccessToken.Token}");
	}

	[Fact]
	public async Task Create_SetsExpectedContentOnDurableRequest()
	{
		var durableRequest = await _evaluateDocumentHttpRequestFactory.Create(_caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId);

		durableRequest.Content.Should().Be(_content);
	}

	[Fact]
	public async Task Create_ClientCredentialsFlow_ThrowsExceptionWhenExceptionOccurs()
	{
		_mockIdentityClientAdapter.Setup(x => x.GetClientAccessTokenAsync(It.IsAny<string>(), It.IsAny<Guid>()))
			.Throws(new Exception());

		await Assert.ThrowsAsync<GeneratePdfHttpRequestFactoryException>(() => _evaluateDocumentHttpRequestFactory.Create(_caseId, _documentId, _materialId, _lastUpdatedDate, _correlationId));
	}
	}
}

