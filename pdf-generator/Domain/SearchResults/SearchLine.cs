using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace pdf_generator.Domain.SearchResults;

public class SearchLine : Line
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("caseId")]
    public int CaseId { get; set; }

    [JsonProperty("documentId")]
    public string DocumentId { get; set; }
    
    [JsonProperty("lastUpdatedDate")]
    public string LastUpdatedDate { get; set; }
    
    [JsonProperty("pageIndex")]
    public int PageIndex { get; set; }

    [JsonProperty("lineIndex")]
    public int LineIndex { get; set; }

    [JsonProperty("pageHeight")]
    public double PageHeight { get; set; }

    [JsonProperty("pageWidth")]
    public double PageWidth { get; set; }
}
