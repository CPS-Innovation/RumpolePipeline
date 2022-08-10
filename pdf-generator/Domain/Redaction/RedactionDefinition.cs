using System.Collections.Generic;

namespace pdf_generator.Domain.Redaction
{
    public class RedactionDefinition
    {
        public int PageIndex { get; set; }

        public double Width { get; set; }

        public int Height { get; set; }

        public List<RedactionCoordinates> RedactionCoordinates { get; set; }
    }
}
