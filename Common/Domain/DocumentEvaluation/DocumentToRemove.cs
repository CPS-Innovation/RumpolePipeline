using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Common.Domain.DocumentEvaluation;

public class DocumentToRemove
{
    public DocumentToRemove(string documentId, long versionId)
    {
        DocumentId = documentId;
        VersionId = versionId;
    }
    
    [JsonProperty("documentId")]
    [Required]
    public string DocumentId { get; set; }
        
    [JsonProperty("versionId")]
    [Required]
    public long VersionId { get; set; }
}
