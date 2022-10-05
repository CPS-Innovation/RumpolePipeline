using System;
using System.Threading.Tasks;
using coordinator.Domain.DocumentExtraction;

namespace coordinator.Clients
{
	public interface IDocumentExtractionClient
	{
		Task<Case> GetCaseDocumentsAsync(string caseId, string accessToken, Guid correlationId);
	}
}

