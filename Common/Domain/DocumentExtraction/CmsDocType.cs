using Common.Constants;

namespace Common.Domain.DocumentExtraction
{
	public class CmsDocType
	{
		public CmsDocType() { }
		
		public CmsDocType(string documentType, string documentCategory)
		{
			Code = documentType ?? MiscCategories.UnknownDocumentType;
			Name = documentCategory;
		}
		
		public string Code { get; set; }

		public string Name { get; set; }
	}
}

