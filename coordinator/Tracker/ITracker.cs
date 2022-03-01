using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tracker
{
    public interface ITracker
    {
        Task Initialise(string TransactionId);
        Task Register(List<int> documentIds);
        void RegisterPdfUrl(TrackerPdfArg arg);

        void RegisterIsProcessedForSearchAndPageDimensions(TrackerPageArg trackerSearchArg);
        void RegisterIsIndexed();
        Task<ITracker> Get();

        Task<bool> GetIsAlreadyProcessed();

        Task<List<TrackerDocument>> GetDocuments();
    }
}