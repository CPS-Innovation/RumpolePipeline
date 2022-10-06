namespace coordinator.Domain.Tracker
{
    public enum LogType
    {
        Initialised,
        RegisteredDocumentIds,
        RegisteredPdfBlobName,
        UnableToConvertDocumentToPdf,
        DocumentNotFoundInCde,
        UnexpectedDocumentFailure,
        NoDocumentsFoundInCde,
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed
    }
}
