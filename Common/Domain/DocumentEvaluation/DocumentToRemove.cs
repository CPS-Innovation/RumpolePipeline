using System.ComponentModel.DataAnnotations;
using Common.Validators;
using Newtonsoft.Json;

namespace Common.Domain.DocumentEvaluation;

public class DocumentToRemove
{
    public DocumentToRemove(string documentId, long versionId, string pdfBlobName)
    {
        DocumentId = documentId;
        VersionId = versionId;
        PdfBlobName = pdfBlobName;
    }
    
    [JsonProperty("documentId")]
    [Required]
    public string DocumentId { get; set; }
        
    [JsonProperty("versionId")]
    [RequiredLongGreaterThanZero]
    public long VersionId { get; set; }
    
    [JsonProperty("pdfBlobName")]
    [Required]
    public string PdfBlobName { get; set; }
}
