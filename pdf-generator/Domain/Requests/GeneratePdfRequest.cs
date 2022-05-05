using System.ComponentModel.DataAnnotations;

namespace pdf_generator.Domain.Requests
{
    public class GeneratePdfRequest
    {
        [Required]
        public int? CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }

        [Required]
        [RegularExpression(@"^[\w,\s-]+\.[A-Za-z]{3,4}$")]
        public string FileName { get; set; }
    }
}