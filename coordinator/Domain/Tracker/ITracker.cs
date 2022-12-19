using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentEvaluation;

// ReSharper disable InconsistentNaming

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        Task Initialise(string transactionId);
        Task<DocumentEvaluationActivityPayload> RegisterDocumentIds(string caseUrn, long caseId, List<IncomingDocument> incomingDocuments, Guid correlationId);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg);
        Task RegisterDocumentNotFoundInDDEI(string documentId);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterNoDocumentsFoundInDDEI();
        Task ProcessEvaluatedDocuments();
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> AllDocumentsFailed();
        Task<bool> IsAlreadyProcessed();
        Task<bool> IsStale(bool forceRefresh);
    }
}