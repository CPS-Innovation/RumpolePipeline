using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;

namespace coordinator.Clients
{
	public class DocumentExtractionClient : IDocumentExtractionClient
	{
		public DocumentExtractionClient()
		{
		}

        public Task<Case> GetCaseDocumentsAsync(string caseId, string accessToken, Guid correlationId)
        {
            // TODO
            throw new NotImplementedException();
        }
    }
}

