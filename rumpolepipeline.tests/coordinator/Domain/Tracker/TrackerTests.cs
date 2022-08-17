﻿using AutoFixture;
using coordinator.Domain.Tracker;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace rumpolepipeline.tests.coordinator.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly string _transactionId;
        private readonly IEnumerable<string> _documentIds;
        private readonly RegisterPdfBlobNameArg _pdfBlobNameArg;
        private readonly List<TrackerDocument> _trackerDocuments;
        private readonly string _caseId;
        private EntityStateResponse<global::coordinator.Domain.Tracker.Tracker> _entityStateResponse;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly global::coordinator.Domain.Tracker.Tracker _tracker;

        public TrackerTests()
        {
            var fixture = new Fixture();
            _transactionId = fixture.Create<string>();
            _documentIds = fixture.Create<IEnumerable<string>>();
            _pdfBlobNameArg = fixture.Build<RegisterPdfBlobNameArg>()
                                .With(a => a.DocumentId, _documentIds.First())
                                .Create();
            _trackerDocuments = fixture.Create<List<TrackerDocument>>();
            _caseId = fixture.Create<string>();
            _entityStateResponse = new EntityStateResponse<global::coordinator.Domain.Tracker.Tracker>() { EntityExists = true };

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<global::coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(global::coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId),
                    null, null))
                .ReturnsAsync(_entityStateResponse);

            _tracker = new global::coordinator.Domain.Tracker.Tracker();
        }

        [Fact]
        public async Task Initialise_Initializes()
        {
            await _tracker.Initialise(_transactionId);

            _tracker.TransactionId.Should().Be(_transactionId);
            _tracker.Documents.Should().NotBeNull();
            _tracker.Logs.Should().NotBeNull();
            _tracker.Status.Should().Be(TrackerStatus.Running);

            _tracker.Logs.Count.Should().Be(1);
        }

        [Fact]
        public async Task RegisterDocumentIds_RegistersDocumentIds()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);

            _tracker.Documents.Count.Should().Be(_documentIds.Count());

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterPdfBlobName(_pdfBlobNameArg);

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);
            document?.Status.Should().Be(DocumentStatus.PdfUploadedToBlob);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterDocumentNotFoundInCDE_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterDocumentNotFoundInCDE(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.Status.Should().Be(DocumentStatus.NotFoundInCDE);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterFailedToConvertToPdf_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterUnableToConvertDocumentToPdf(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.Status.Should().Be(DocumentStatus.UnableToConvertToPdf);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterUnexpectedDocumentFailure_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterUnexpectedPdfDocumentFailure(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.Status.Should().Be(DocumentStatus.UnexpectedFailure);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterNoDocumentsFoundInCDE_RegistersNoDocumentsFoundInCDE()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterNoDocumentsFoundInCDE();

            _tracker.Status.Should().Be(TrackerStatus.NoDocumentsFoundInCDE);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersIndexed()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterIndexed(_documentIds.First());

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.Status.Should().Be(DocumentStatus.Indexed);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersOcrAndIndexFailure()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_documentIds);
            await _tracker.RegisterOcrAndIndexFailure(_documentIds.First());

            var document = _tracker.Documents.Find(document => document.DocumentId == _documentIds.First());
            document?.Status.Should().Be(DocumentStatus.OcrAndIndexFailure);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterCompleted_RegistersCompleted()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterCompleted();

            _tracker.Status.Should().Be(TrackerStatus.Completed);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterFailed_RegistersFailed()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterFailed();

            _tracker.Status.Should().Be(TrackerStatus.Failed);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            _tracker.Documents = _trackerDocuments;
            var documents = await _tracker.GetDocuments();

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _tracker.Documents = new List<TrackerDocument> {
                new() { Status = DocumentStatus.NotFoundInCDE},
                new() { Status = DocumentStatus.UnableToConvertToPdf},
                new() { Status = DocumentStatus.UnexpectedFailure}
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _tracker.Documents = new List<TrackerDocument> {
                new() { Status = DocumentStatus.NotFoundInCDE},
                new() { Status = DocumentStatus.UnableToConvertToPdf},
                new() { Status = DocumentStatus.UnexpectedFailure},
                new() { Status = DocumentStatus.PdfUploadedToBlob},
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsTrueIfStatusIsCompleted()
        {
            _tracker.Status = TrackerStatus.Completed;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsTrueIfStatusIsNoDocumentsFoundInCDE()
        {
            _tracker.Status = TrackerStatus.NoDocumentsFoundInCDE;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsFalseIfStatusIsNotCompletedAndNotNoDocumentsFoundInCDE()
        {
            _tracker.Status = TrackerStatus.NotStarted;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await global::coordinator.Domain.Tracker.Tracker.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<global::coordinator.Domain.Tracker.Tracker>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            var response = await _tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsEntityState()
        {
            var response  = await _tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().Be(_entityStateResponse.EntityState);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            var entityStateResponse = new EntityStateResponse<global::coordinator.Domain.Tracker.Tracker>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<global::coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(global::coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId),
                    null, null))
                .ReturnsAsync(entityStateResponse);

            var response = await _tracker.HttpStart(new HttpRequestMessage(), _caseId, _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}