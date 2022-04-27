using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain.Tracker;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private Fixture _fixture;
        private string _transactionId;
        private IEnumerable<string> _documentIds;
        private RegisterPdfBlobNameArg _pdfBlobNameArg;
        private List<TrackerDocument> _trackerDocuments;
        private string _caseId;
        private EntityStateResponse<coordinator.Domain.Tracker.Tracker> _entityStateResponse;

        private Mock<IDurableEntityContext> _mockDurableEntityContext;
        private Mock<IDurableEntityClient> _mockDurableEntityClient;
        private Mock<ILogger> _mockLogger;

        private coordinator.Domain.Tracker.Tracker Tracker;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _documentIds = _fixture.Create<IEnumerable<string>>();
            _pdfBlobNameArg = _fixture.Build<RegisterPdfBlobNameArg>()
                                .With(a => a.DocumentId, _documentIds.First())
                                .Create();
            _trackerDocuments = _fixture.Create<List<TrackerDocument>>();
            _caseId = _fixture.Create<string>();
            _entityStateResponse = new EntityStateResponse<coordinator.Domain.Tracker.Tracker>() { EntityExists = true };

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId),
                    null, null))
                .ReturnsAsync(_entityStateResponse);

            Tracker = new coordinator.Domain.Tracker.Tracker();
        }

        [Fact]
        public async Task Initialise_Initialises()
        {
            await Tracker.Initialise(_transactionId);

            Tracker.TransactionId.Should().Be(_transactionId);
            Tracker.Documents.Should().NotBeNull();
            Tracker.Logs.Should().NotBeNull();
            Tracker.IsComplete.Should().BeFalse();

            Tracker.Logs.Count().Should().Be(1);
        }

        [Fact]
        public async Task RegisterDocumentIds_RegistersDocumentIds()
        {
            await Tracker.Initialise(_transactionId);
            await Tracker.RegisterDocumentIds(_documentIds);

            Tracker.Documents.Count().Should().Be(_documentIds.Count());

            Tracker.Logs.Count().Should().Be(2);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            await Tracker.Initialise(_transactionId);
            await Tracker.RegisterDocumentIds(_documentIds);
            await Tracker.RegisterPdfBlobName(_pdfBlobNameArg);

            var document = Tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);

            Tracker.Logs.Count().Should().Be(3);
        }

        [Fact]
        public async Task RegisterCompleted_RegistersCompleted()
        {
            await Tracker.Initialise(_transactionId);
            await Tracker.RegisterDocumentIds(_documentIds);
            await Tracker.RegisterPdfBlobName(_pdfBlobNameArg);
            await Tracker.RegisterCompleted();

            var document = Tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);
            Tracker.IsComplete.Should().BeTrue();

            Tracker.Logs.Count().Should().Be(4);
        }

        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            Tracker.Documents = _trackerDocuments;
            var documents = await Tracker.GetDocuments();

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsTrueIfStatusIsCompleted()
        {
            Tracker.IsComplete = true;

            var isAlreadyProcessed = await Tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsFalseIfStatusIsNotCompleted()
        {
            Tracker.IsComplete = false;

            var isAlreadyProcessed = await Tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await Tracker.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<coordinator.Domain.Tracker.Tracker>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            var response = await Tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsEntityState()
        {
            var response  = await Tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult.Value.Should().Be(_entityStateResponse.EntityState);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            var entityStateResponse = new EntityStateResponse<coordinator.Domain.Tracker.Tracker>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId),
                    null, null))
                .ReturnsAsync(entityStateResponse);

            var response = await Tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
