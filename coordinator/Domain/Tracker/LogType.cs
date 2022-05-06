namespace coordinator.Domain.Tracker
{
    public enum LogType
    {
        Initialised,
        RegisteredDocumentIds,
        RegisteredPdfBlobName,
        UnableToConvertDocumentToPdf,
        DocumentNotFoundInCDE,
        UnexpectedDocumentFailure,
        NoDocumentsFoundInCDE,
        Completed,
        Failed
    }
}
