using System;
using System.IO;
using System.Threading.Tasks;

namespace pdf_generator.Services.DocumentExtractionService
{
	public class DocumentExtractionServiceStub : IDocumentExtractionService
	{
		public DocumentExtractionServiceStub()
		{
		}

        public Task<Stream> GetDocumentAsync(string documentId, string fileName, string accessToken)
        {
            //TODO get documents from cms blob storage
            //TODO upload documents to blob storage
            throw new NotImplementedException();
        }
    }
}

