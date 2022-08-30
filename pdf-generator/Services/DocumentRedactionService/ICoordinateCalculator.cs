﻿using Aspose.Pdf.Facades;
using pdf_generator.Domain.Redaction;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface ICoordinateCalculator
    {
        RedactionCoordinates CalculateRelativeCoordinates(double pageWidth, double pageHeight, int pageIndex, RedactionCoordinates originatorCoordinates, PdfFileInfo targetPdfInfo);
    }
}
