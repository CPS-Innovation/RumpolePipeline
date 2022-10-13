using System.Diagnostics.Contracts;

namespace Common.Constants
{
    public static class AuthenticationKeys
    {
        public const string Authorization = "Authorization";
        public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
        public const string AzureAuthenticationAssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public const string Bearer = "Bearer";
    }

    public static class EventGridEvents
    {
        public const string SubscriptionValidationEvent = "Microsoft.EventGrid.SubscriptionValidationEvent";
        public const string BlobDeletedEvent = "Microsoft.Storage.BlobDeleted";
    }

    public static class PipelineRoles
    {
        public const string ExtractText = "application.extracttext";
        public const string HandlePolarisDocumentDeleted = "";
    }

    public static class PipelineScopes
    {
        public const string ExtractText = "";
        public const string HandlePolarisDocumentDeleted = "";
    }
}
