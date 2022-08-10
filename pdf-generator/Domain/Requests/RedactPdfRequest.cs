using System.Collections.Generic;
using pdf_generator.Domain.Redaction;

namespace pdf_generator.Domain.Requests
{
    public class RedactPdfRequest
    {
        public string CaseId { get; set; }

        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinition> RedactionDefinitions { get; set; }
    }
}
