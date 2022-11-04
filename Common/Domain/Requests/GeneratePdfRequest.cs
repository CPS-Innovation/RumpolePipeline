using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class GeneratePdfRequest
    {
        public GeneratePdfRequest(int caseId, string documentId, string fileName, string lastUpdatedDate)
        {
            CaseId = caseId;
            DocumentId = documentId;
            FileName = fileName;
            LastUpdatedDate = lastUpdatedDate;
        }
        
        [Required]
        public int? CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string FileName { get; set; }
        
        [Required]
        public string LastUpdatedDate { get; set; }
    }
}