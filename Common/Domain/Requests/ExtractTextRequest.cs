using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class ExtractTextRequest
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        [Required] 
        public string MaterialId { get; set; }
        
        [Required] 
        public string LastUpdatedDate { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}