using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class GeneratePdfRequest
    {
        [Required]
        public int? CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string FileName { get; set; }
        
        [Required(AllowEmptyStrings = true)]
        public string MaterialId { get; set; }

        [Required]
        public string LastUpdatedDate { get; set; }
    }
}