using Azure.Search.Documents;

namespace text_extractor.Factories
{
	public interface ISearchClientFactory
	{
		SearchClient Create();
	}
}

