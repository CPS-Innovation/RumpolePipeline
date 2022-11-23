using System;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace text_extractor.Services.SearchIndexService
{
	public interface ISearchIndexService
	{
		Task StoreResultsAsync(AnalyzeResults analyzeresults, long caseId, string documentId, long versionId, Guid correlationId);

		Task RemoveResultsForDocumentAsync(long caseId, string documentId, Guid correlationId);
	}
}

