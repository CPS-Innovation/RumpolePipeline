using System.Collections.Generic;
using Common.Domain.Redaction;

namespace Common.Domain.Requests
{
    public class RedactPdfRequest
    {
        public RedactPdfRequest(string caseId, string documentId, string lastUpdateDate, string fileName, List<RedactionDefinition> redactionDefinitions)
        {
            CaseId = caseId;
            DocumentId = documentId;
            LastUpdateDate = lastUpdateDate;
            FileName = fileName;
            RedactionDefinitions = redactionDefinitions;
        }
        
        public string CaseId { get; set; }

        public string DocumentId { get; set; }
        
        public string LastUpdateDate { get; set; }

        public string FileName { get; set; }

        public List<RedactionDefinition> RedactionDefinitions { get; set; }
    }
}
