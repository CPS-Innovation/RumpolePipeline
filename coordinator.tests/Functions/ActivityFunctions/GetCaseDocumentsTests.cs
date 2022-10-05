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
        private readonly Case _case;

        private readonly Mock<IDurableActivityContext> _mockDurableActivityContext;

        private readonly GetCaseDocuments _getCaseDocuments;

        public GetCaseDocumentsTests()
        {
            var fixture = new Fixture();
            var payload = fixture.Create<GetCaseDocumentsActivityPayload>();
            _case = fixture.Create<Case>();

            var mockDocumentExtractionClient = new Mock<IDocumentExtractionClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(payload);

            mockDocumentExtractionClient.Setup(client => client.GetCaseDocumentsAsync(payload.CaseId.ToString(), payload.AccessToken, payload.CorrelationId))
                .ReturnsAsync(_case);

            _getCaseDocuments = new GetCaseDocuments(mockDocumentExtractionClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsActivityPayload>())
                .Returns(default(GetCaseDocumentsActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _getCaseDocuments.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsCaseDocuments()
        {
            var caseDocuments = await _getCaseDocuments.Run(_mockDurableActivityContext.Object);

            caseDocuments.Should().BeEquivalentTo(_case.CaseDocuments);
        }
    }
}
