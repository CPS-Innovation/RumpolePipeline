using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public interface ISearchLineFactory
	{
		SearchLine Create(int caseId, string documentId, string lastUpdatedDate, ReadResult readResult, Line line, int index);
	}
}

