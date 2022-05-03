using System.IO;
using Aspose.Cells;
using Aspose.Diagram;
using Aspose.Email;
using Aspose.Slides;
using Aspose.Words;

namespace pdf_generator.Factories
{
	public interface IAsposeItemFactory
	{
		public Workbook CreateWorkbook(Stream inputStream);

		public Diagram CreateDiagram(Stream inputStream);

		public MailMessage CreateMailMessage(Stream inputStream);

		public Document CreateMhtmlDocument(Stream inputStream);

		public Aspose.Pdf.Document CreateHtmlDocument(Stream inputStream);

		public Aspose.Imaging.Image CreateImage(Stream inputStream);

		public Presentation CreatePresentation(Stream inputStream);

		public Document CreateWordsDocument(Stream inputStream);
	}
}

