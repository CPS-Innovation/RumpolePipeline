using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Domain.Tracker
{
    public interface ITracker
    {
        void Initialise(string transactionId); //TODO do we need transaction id?
        void RegisterDocumentIds(List<int> documentIds);
        void RegisterPdfBlobName(RegisterPdfBlobNameArg arg);
        void RegisterCompleted();
        Task<ITracker> Get();
        Task<List<TrackerDocument>> GetDocuments();
        Task<bool> IsAlreadyProcessed();

        //TODO make them all tasks?
    }
}