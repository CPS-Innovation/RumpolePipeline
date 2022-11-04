using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class ExtractTextRequest
    {
        public ExtractTextRequest(int caseId, string documentId, string lastUpdatedDate, string blobName)
        {
            CaseId = caseId;
            DocumentId = documentId;
            LastUpdatedDate = lastUpdatedDate;
            BlobName = blobName;
        }
        
        [Required]
        public int CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        [Required] 
        public string LastUpdatedDate { get; set; }
        
        [Required]
        public string BlobName { get; set; }
    }
}