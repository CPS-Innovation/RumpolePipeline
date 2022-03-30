using System;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.CoreDataApi;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetCaseDetailsByIdTests
    {
        private Fixture _fixture;
        private GetCaseDetailsByIdActivityPayload _payload;
        private CaseDetails _caseDetails;

        private Mock<ICoreDataApiClient> _mockCoreDataApiClient;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private GetCaseDetailsById GetCaseDetailsById;

        public GetCaseDetailsByIdTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Create<GetCaseDetailsByIdActivityPayload>();
            _caseDetails = _fixture.Create<CaseDetails>();

            _mockCoreDataApiClient = new Mock<ICoreDataApiClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDetailsByIdActivityPayload>())
                .Returns(_payload);

            _mockCoreDataApiClient.Setup(client => client.GetCaseDetailsByIdAsync(_payload.CaseId, _payload.AccessToken))
                .ReturnsAsync(_caseDetails);

            GetCaseDetailsById = new GetCaseDetailsById(_mockCoreDataApiClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDetailsByIdActivityPayload>())
                .Returns(default(GetCaseDetailsByIdActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => GetCaseDetailsById.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsCaseDetails()
        {
            var caseDetails = await GetCaseDetailsById.Run(_mockDurableActivityContext.Object);

            caseDetails.Should().Be(_caseDetails);
        }
    }
}
