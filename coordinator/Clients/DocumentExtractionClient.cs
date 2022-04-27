using System;
using System.Threading.Tasks;
using coordinator.Domain.DocumentExtraction;

namespace coordinator.Clients
{
	public class DocumentExtractionClient : IDocumentExtractionClient
	{
		public DocumentExtractionClient()
		{
		}

        public Task<Case> GetCaseDocumentsAsync(string caseId, string accessToken)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}

