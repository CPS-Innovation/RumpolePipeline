using System;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Clients;
using coordinator.Domain;
using coordinator.Domain.DocumentExtraction;
using coordinator.Functions.ActivityFunctions;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Functions.ActivityFunctions
{
    public class GetCaseDocumentsTests
    {
        private Fixture _fixture;
        private GetCaseDocumentsActivityPayload _payload;
        private Case _case;

        private Mock<IDocumentExtractionClient> _mockDocumentExtractionClient;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private GetCaseDocuments GetCaseDocuments;

        public GetCaseDocumentsTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Create<GetCaseDocumentsActivityPayload>();
            _case = _fixture.Create<Case>();

            _mockDocumentExtractionClient = new Mock<IDocumentExtractionClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(_payload);

            _mockDocumentExtractionClient.Setup(client => client.GetCaseDocumentsAsync(_payload.CaseId.ToString(), _payload.AccessToken))
                .ReturnsAsync(_case);

            GetCaseDocuments = new GetCaseDocuments(_mockDocumentExtractionClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(default(GetCaseDocumentsActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => GetCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsCaseDocuments()
        {
            var caseDocuments = await GetCaseDocuments.Run(_mockDurableActivityContext.Object);

            caseDocuments.Should().BeEquivalentTo(_case.CaseDocuments);
        }
    }
}
