namespace Common.Domain.DocumentExtraction
{
	public class CaseDocument
	{
		public string DocumentId { get; set; }

		public long VersionId { get; set; }

		public string FileName { get; set; }

		public CmsDocType CmsDocType { get; set; }
	}
}

