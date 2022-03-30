using System;
using AutoFixture;
using coordinator.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Moq;

namespace coordinator.tests.Clients
{
    public class OnBehalfOfTokenClientTests
    {
        private Fixture _fixture;
        private string _accessToken;

        private Mock<IConfidentialClientApplication> _mockConfidentialClientApplication;
        private Mock<IConfiguration> _mockConfiguration;

        private IOnBehalfOfTokenClient OnBehalfOfTokenClient;

        public OnBehalfOfTokenClientTests()
        {
            _fixture = new Fixture();
            _accessToken = _fixture.Create<string>();

            _mockConfidentialClientApplication = new Mock<IConfidentialClientApplication>();
            _mockConfiguration = new Mock<IConfiguration>();

            OnBehalfOfTokenClient = new OnBehalfOfTokenClient(_mockConfidentialClientApplication.Object, _mockConfiguration.Object);
        }
    }
}
