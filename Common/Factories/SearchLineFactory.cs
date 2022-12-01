using System;
using System.Text;
using Common.Domain.SearchIndex;
using Common.Factories.Contracts;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Common.Factories
{
	public class SearchLineFactory : ISearchLineFactory
	{
        public SearchLine Create(long caseId, string documentId, long versionId, string blobName, ReadResult readResult, Line line, int index)
        {
            var id = $"{caseId}-{documentId}-{readResult.Page}-{index}";
            var bytes = Encoding.UTF8.GetBytes(id);
            var base64Id = Convert.ToBase64String(bytes);

            return new SearchLine
            {
                Id = base64Id,
                CaseId = caseId,
                DocumentId = documentId,
                VersionId = versionId,
                FileName = blobName,
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

