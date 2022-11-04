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
        Indexed,
        OcrAndIndexFailure,
        Completed,
        Failed, 
        DocumentEvaluated,
        UnexpectedDocumentEvaluationFailure, 
        UnableToEvaluateDocument,
        DocumentRemovedFromSearchIndex,
        IndexRemovalFailure,
        UnexpectedExistingDocumentsEvaluationFailure
    }
}
