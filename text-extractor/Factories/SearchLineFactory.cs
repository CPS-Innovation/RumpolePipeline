using System;
using System.Text;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public class SearchLineFactory : ISearchLineFactory
	{
        public SearchLine Create(int caseId, string documentId, string materialId, string lastUpdatedDate, ReadResult readResult, Line line, int index)
        {
            var id = $"{caseId}-{documentId}-{readResult.Page}-{index}";
            var bytes = Encoding.UTF8.GetBytes(id);
            var base64Id = Convert.ToBase64String(bytes);

            return new SearchLine
            {
                Id = base64Id,
                CaseId = caseId,
                DocumentId = documentId,
                MaterialId = materialId,
                LastUpdatedDate = lastUpdatedDate,
                PageIndex = readResult.Page,
                LineIndex = index,
                Language = line.Language,
                BoundingBox = line.BoundingBox,
                Appearance = line.Appearance,
                Text = line.Text,
                Words = line.Words,
                PageHeight = readResult.Height,
                PageWidth = readResult.Width
            };
        }
	}
}

