namespace coordinator.Domain.Tracker
{
    public enum LogType
    {
        Initialised,
        RegisteredDocumentIds,
        RegisteredPdfBlobName,
        FailedToConvertToPdf,
        DocumentNotFoundInCDE,
        Completed,
        Failed
    }
}
