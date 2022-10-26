using Azure.Search.Documents;

namespace pdf_generator.Factories;

public interface ISearchClientFactory
{
    SearchClient Create();
}
