using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class ExtractTextRequest
    {
        public ExtractTextRequest(long caseId, string documentId, long versionId, string blobName)
        {
            CaseId = caseId;
            DocumentId = documentId;
            VersionId = versionId;
            BlobName = blobName;
        }
        
        [Required]
        public long CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        [Required] 
        public long VersionId { get; set; }
        
        [Required]
        public string BlobName { get; set; }
    }
}