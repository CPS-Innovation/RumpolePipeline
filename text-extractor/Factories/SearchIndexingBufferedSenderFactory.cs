using Azure.Search.Documents;
using text_extractor.Domain;

namespace text_extractor.Factories
{
	public class SearchIndexingBufferedSenderFactory: ISearchIndexingBufferedSenderFactory
	{
		public SearchIndexingBufferedSender<SearchLine> Create(SearchClient searchClient)
        {
			return new SearchIndexingBufferedSender<SearchLine>(searchClient,
				new SearchIndexingBufferedSenderOptions<SearchLine>
					{
						KeyFieldAccessor = searchLine => searchLine.Id
					});
		}
	}
}

