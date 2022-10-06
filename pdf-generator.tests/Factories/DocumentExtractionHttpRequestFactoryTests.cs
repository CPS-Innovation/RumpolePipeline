using System;
using System.Net.Http;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Factories;
using Xunit;

namespace pdf_generator.tests.Factories
{
	public class DocumentExtractionHttpRequestFactoryTests
	{
        private readonly string _requestUri;
        private readonly string _accessToken;
        private readonly Guid _correlationId;

        private readonly IDocumentExtractionHttpRequestFactory _documentExtractionHttpRequestFactory;

        public DocumentExtractionHttpRequestFactoryTests()
        {
            var fixture = new Fixture();
            _requestUri = fixture.Create<string>();
            _accessToken = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();

            var loggerMock = new Mock<ILogger<DocumentExtractionHttpRequestFactory>>();
            
            _documentExtractionHttpRequestFactory = new DocumentExtractionHttpRequestFactory(loggerMock.Object);
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.Create(_requestUri, _accessToken, _correlationId);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = _documentExtractionHttpRequestFactory.Create(_requestUri, _accessToken, _correlationId);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = _documentExtractionHttpRequestFactory.Create(_requestUri, _accessToken, _correlationId);

            message.Headers.Authorization?.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}

