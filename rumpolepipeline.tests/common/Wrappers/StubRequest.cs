using System.ComponentModel.DataAnnotations;

namespace rumpolepipeline.tests.common.Wrappers
{
	public class StubRequest
	{
		[Required]
		public string StubString { get; set; } = string.Empty;
	}
}

