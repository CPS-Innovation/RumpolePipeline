﻿using System;

namespace coordinator.Domain
{
    public class CreateGeneratePdfHttpRequestActivityPayload : BasePipelinePayload
    {
        public CreateGeneratePdfHttpRequestActivityPayload(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId, Guid correlationId)
            : base(caseUrn, caseId, correlationId)
        {
            DocumentId = documentId;
            DocumentCategory = documentCategory;
            FileName = fileName;
            VersionId = versionId;
        }

        public string DocumentCategory { get; set; }
        
        public string DocumentId { get; set; }

        public string FileName { get; set; }

        public long VersionId { get; set; }
    }
}
