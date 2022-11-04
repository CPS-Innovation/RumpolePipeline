using System.Collections.Generic;

namespace Common.Domain.DocumentEvaluation;

public class DocumentInformation
{
    public string BlobName { get; set; }

    public string BlobContainerName { get; set; }

    public Dictionary<string, string> DocumentMetadata { get; set; }
}
