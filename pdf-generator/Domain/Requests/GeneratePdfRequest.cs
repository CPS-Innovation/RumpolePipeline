using System.ComponentModel.DataAnnotations;

namespace pdf_generator.Domain.Requests
{
    public class GeneratePdfRequest
    {
        [Required]
        public int CaseId { get; set; }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        public string FileName { get; set; }

        public string BlobLink { get; set; }
    }
}