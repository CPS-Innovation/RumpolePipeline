using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public interface ISearchLineFactory
	{
		SearchLine Create(long caseId, string documentId, long versionId, ReadResult readResult, Line line, int index);
	}
}

