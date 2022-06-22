using System;
using Azure.Search.Documents;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public interface ISearchIndexingBufferedSenderFactory
	{
		SearchIndexingBufferedSender<SearchLine> Create(SearchClient searchClient);
	}
}

