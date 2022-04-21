using System;
using System.Collections.Generic;
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
    public class GetCaseDocumentsByIdTests
    {
        private Fixture _fixture;
        private GetCaseDocumentsByIdActivityPayload _payload;
        private List<Document> _documents;

        private Mock<ICoreDataApiClient> _mockCoreDataApiClient;
        private Mock<IDurableActivityContext> _mockDurableActivityContext;

        private GetCaseDocumentsById GetCaseDocumentsById;

        public GetCaseDocumentsByIdTests()
        {
            _fixture = new Fixture();
            _payload = _fixture.Create<GetCaseDocumentsByIdActivityPayload>();
            _documents = _fixture.Create<List<Document>>();

            _mockCoreDataApiClient = new Mock<ICoreDataApiClient>();
            _mockDurableActivityContext = new Mock<IDurableActivityContext>();

            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsByIdActivityPayload>())
                .Returns(_payload);

            _mockCoreDataApiClient.Setup(client => client.GetCaseDocumentsByIdAsync(_payload.CaseId, _payload.AccessToken))
                .ReturnsAsync(_documents);

            GetCaseDocumentsById = new GetCaseDocumentsById(_mockCoreDataApiClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableActivityContext.Setup(context => context.GetInput<GetCaseDocumentsByIdActivityPayload>())
                .Returns(default(GetCaseDocumentsByIdActivityPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => GetCaseDocumentsById.Run(_mockDurableActivityContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsCaseDocuments()
        {
            var documents = await GetCaseDocumentsById.Run(_mockDurableActivityContext.Object);

            documents.Should().BeEquivalentTo(_documents);
        }
    }
}
