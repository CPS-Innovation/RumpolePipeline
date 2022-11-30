namespace Common.Domain.DocumentExtraction
{
	public class CaseDocument
	{
		public CaseDocument() { }
		
		public CaseDocument(string documentId, long versionId, string fileName, string documentType, string documentCategory)
		{
			DocumentId = documentId;
			VersionId = versionId;
			FileName = fileName;
			CmsDocType = new CmsDocType(documentType, documentCategory);
		}

		public string DocumentId { get; set; }

		public long VersionId { get; set; }

		public string FileName { get; set; }

		public CmsDocType CmsDocType { get; set; }
	}
}

