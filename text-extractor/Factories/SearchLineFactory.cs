using System;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public class SearchLineFactory : ISearchLineFactory
	{
        public SearchLine Create(int caseId, string documentId, ReadResult readResult, Line line, int index)
        {
            return new SearchLine
            {
                Id = $"{caseId}-{documentId}-{readResult.Page}-{index}",
                CaseId = caseId,
                DocumentId = documentId,
                PageIndex = readResult.Page,
                LineIndex = index,
                Language = line.Language,
                BoundingBox = line.BoundingBox,
                Appearance = line.Appearance,
                Text = line.Text,
                Words = line.Words
            };
        }
	}
}

