using System.Collections.Generic;
using Common.Domain.Redaction;

namespace Common.Domain.Requests
{
    public class RedactPdfRequest
    {
        public string CaseId { get; set; }

        public string DocumentId { get; set; }
        

        public string MaterialId { get; set; }
        

        public string LastUpdateDate { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinition> RedactionDefinitions { get; set; }
    }
}
