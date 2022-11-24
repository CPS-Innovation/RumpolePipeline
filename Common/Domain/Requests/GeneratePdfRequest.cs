﻿using System.ComponentModel.DataAnnotations;

namespace Common.Domain.Requests
{
    public class GeneratePdfRequest
    {
        public GeneratePdfRequest(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId)
        {
            CaseUrn = caseUrn;
            CaseId = caseId;
            DocumentCategory = documentCategory;
            DocumentId = documentId;
            FileName = fileName;
            VersionId = versionId;
        }
        
        [Required] 
        public string CaseUrn { get; set; }
        
        [Required]
        [Range(1, long.MaxValue)]
        public long CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }
        
        public string DocumentCategory { get; set; }
        
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string FileName { get; set; }
        
        [Required]
        [Range(1, long.MaxValue)]
        public long VersionId { get; set; }
    }
}