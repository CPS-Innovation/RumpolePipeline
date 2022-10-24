using System.Collections.Generic;

namespace Common.Domain.DocumentEvaluation;

public class TaggedBlobItemWrapper
{
    public string BlobItemName { get; set; }

    public string BlobItemContainerName { get; set; }

    public Dictionary<string, string> BlobItemTags { get; set; }
}
