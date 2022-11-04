using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        Task Initialise(string transactionId);
        Task RegisterDocumentIds(IEnumerable<string> documentIds);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterDocumentNotFoundInCDE(string documentId);
        Task RegisterUnableToConvertDocumentToPdf(string documentId);
        Task RegisterUnexpectedPdfDocumentFailure(string documentId);
        Task RegisterNoDocumentsFoundInCDE();
        Task RegisterDocumentEvaluated(string documentId);
        Task RegisterUnexpectedDocumentEvaluationFailure(string documentId);
        Task RegisterUnableToEvaluateDocument(string documentId);
        Task RegisterIndexed(string documentId);
        Task RegisterOcrAndIndexFailure(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task RegisterDocumentRemovedFromSearchIndex(string documentId);
        Task RegisterUnexpectedSearchIndexRemovalFailure(string documentId);
        Task RegisterUnableToUpdateSearchIndex(string documentId);
        Task RegisterUnexpectedExistingDocumentsEvaluationFailure();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> AllDocumentsFailed();
        Task<bool> IsAlreadyProcessed();
    }
}