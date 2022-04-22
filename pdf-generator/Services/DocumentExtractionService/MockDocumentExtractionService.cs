using System;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.DocumentExtractionService
{
	public class MockDocumentExtractionService : IDocumentExtractionService
	{
		public MockDocumentExtractionService()
		{
		}

        public Task<Stream> GetDocumentAsync(string documentId, string fileName)
        {
            //TODO get documents from cms blob storage
            //TODO upload documents to blob storage
            throw new NotImplementedException();
        }
    }
}

