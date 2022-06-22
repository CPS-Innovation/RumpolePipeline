using System.ComponentModel.DataAnnotations;

namespace text_extractor.Domain.Requests
{
    public class ExtractTextRequest
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}