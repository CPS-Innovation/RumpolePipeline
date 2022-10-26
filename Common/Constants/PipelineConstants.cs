namespace Common.Constants
{
    public static class AuthenticationKeys
    {
        public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
        public const string AzureAuthenticationAssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public const string Bearer = "Bearer";
    }

    public static class HttpHeaderKeys
    {
        public const string Authorization = "Authorization";
        public const string ContentType = "Content-Type";
        public const string CorrelationId = "Correlation-Id";
    }

    public static class HttpHeaderValues
    {
        public const string ApplicationJson = "application/json";
        public const string AuthTokenType = "Bearer";
    }

    public static class EventGridEvents
    {
        public const string BlobDeletedEvent = "Microsoft.Storage.BlobDeleted";
    }

    public static class PipelineRoles
    {
        public const string GeneratePdf = "";
        public const string RedactPdf = "";
        public const string EvaluateDocument = "";
        public const string EvaluateExistingDocuments = "";
        public const string ExtractText = "application.extracttext";
        public const string HandlePolarisDocumentDeleted = "";
        public const string UpdateSearchIndex = "application.updatesearchindex";
    }

    public static class PipelineScopes
    {
        public const string GeneratePdf = "user_impersonation";
        public const string RedactPdf = "user_impersonation";
        public const string EvaluateDocument = "user_impersonation";
        public const string EvaluateExistingDocuments = "user_impersonation";
        public const string ExtractText = "";
        public const string HandlePolarisDocumentDeleted = "";
        public const string UpdateSearchIndex = "";
    }
    
    public static class DocumentTags
    {
        public const string CaseId = "caseId";
        public const string DocumentId = "documentId";
        public const string MaterialId = "materialId";
        public const string LastUpdatedDate = "lastUpdatedDate";
    }
}
