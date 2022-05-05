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
        Task RegisterFailedToConvertToPdf(string documentId);
        Task RegisterCompleted();
        Task RegisterFailed();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> IsAlreadyProcessed();
    }
}