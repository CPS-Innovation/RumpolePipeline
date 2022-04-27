namespace coordinator.Domain.DocumentExtraction
{
	public class CaseDocument
	{
		public string DocumentId { get; set; }

		public string FileName { get; set; }

		public CmsDocType CmsDocType { get; set; }
	}
}

