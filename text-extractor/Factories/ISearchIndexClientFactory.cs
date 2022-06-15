using System;
using Azure.Search.Documents.Indexes;

namespace text_extractor.Factories
{
	public interface ISearchIndexClientFactory
	{
		SearchIndexClient Create();
	}
}

