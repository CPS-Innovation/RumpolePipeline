using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        Task Initialise(string transactionId); //TODO do we need transaction id?
        Task RegisterDocumentIds(IEnumerable<int> documentIds);
        Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        Task RegisterCompleted();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> IsAlreadyProcessed();
    }
}