namespace Common.Domain.DocumentExtraction
{
	public class CmsDocType
	{
		public CmsDocType() { }
		
		public CmsDocType(string code, string name)
		{
			Code = code;
			Name = name;
		}
		
		public string Code { get; set; }

		public string Name { get; set; }
	}
}

