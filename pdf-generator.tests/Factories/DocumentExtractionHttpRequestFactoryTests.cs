using System.Net.Http;
using AutoFixture;
using FluentAssertions;
using pdf_generator.Factories;
using Xunit;

namespace pdf_generator.tests.Factories
{
	public class DocumentExtractionHttpRequestFactoryTests
	{
        private Fixture _fixture;
        private string _requestUri;
        private string _accessToken;

        private IDocumentExtractionHttpRequestFactory DocumentExtractionHttpRequestFactory;

        public DocumentExtractionHttpRequestFactoryTests()
        {
            _fixture = new Fixture();
            _requestUri = _fixture.Create<string>();
            _accessToken = _fixture.Create<string>();

            DocumentExtractionHttpRequestFactory = new DocumentExtractionHttpRequestFactory();
        }

        [Fact]
        public void Create_SetsHttpMethodToGetOnRequestMessage()
        {
            var message = DocumentExtractionHttpRequestFactory.Create(_requestUri, _accessToken);

            message.Method.Should().Be(HttpMethod.Get);
        }

        [Fact]
        public void Create_SetsRequestUriOnRequestMessage()
        {
            var message = DocumentExtractionHttpRequestFactory.Create(_requestUri, _accessToken);

            message.RequestUri.Should().Be(_requestUri);
        }

        [Fact]
        public void Create_SetsAccessTokenOnRequestMessageAuthorizationHeader()
        {
            var message = DocumentExtractionHttpRequestFactory.Create(_requestUri, _accessToken);

            message.Headers.Authorization.ToString().Should().Be($"Bearer {_accessToken}");
        }
    }
}

