﻿namespace coordinator.Domain
{
    public class CreateGeneratePdfHttpRequestActivityPayload
    {
        public int CaseId { get; set; }

        public string DocumentId { get; set; }

        public string FileName { get; set; }
    }
}