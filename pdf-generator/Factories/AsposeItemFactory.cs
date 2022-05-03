using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Slides;
using Aspose.Words;
using LoadFormat = Aspose.Words.LoadFormat;

namespace pdf_generator.Factories
{
	public class AsposeItemFactory : IAsposeItemFactory
	{
		public Workbook CreateWorkbook(Stream inputStream)
        {
			return new Workbook(inputStream);
        }

		public Diagram CreateDiagram(Stream inputStream)
		{
			return new Diagram(inputStream);
		}

		public MailMessage CreateMailMessage(Stream inputStream)
		{
			return MailMessage.Load(inputStream);
		}

		public Document CreateMhtmlDocument(Stream inputStream)
		{
			return new Document(inputStream, new Aspose.Words.Loading.LoadOptions { LoadFormat = LoadFormat.Mhtml });
		}

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream)
		{
			return new Aspose.Pdf.Document(inputStream, new Aspose.Pdf.HtmlLoadOptions());
		}

		public Aspose.Imaging.Image CreateImage(Stream inputStream)
		{
			return Aspose.Imaging.Image.Load(inputStream);
		}

		public Presentation CreatePresentation(Stream inputStream)
		{
			return new Presentation(inputStream);
		}

		public Document CreateWordsDocument(Stream inputStream)
		{
			return new Document(inputStream);
		}
	}
}

