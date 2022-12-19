using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Common.Domain.DocumentEvaluation;

public class DocumentToUpdate
{
    public DocumentToUpdate(string documentId, long versionId, string pdfBlobName)
    {
        DocumentId = documentId;
        VersionId = versionId;
        PdfBlobName = pdfBlobName;
    }
    
    [JsonProperty("documentId")]
    [Required]
    public string DocumentId { get; set; }
        
    [JsonProperty("versionId")]
    [Required]
    public long VersionId { get; set; }

    [JsonProperty("pdfBlobName")]
    [Required]
    public string PdfBlobName { get; set; }
}
