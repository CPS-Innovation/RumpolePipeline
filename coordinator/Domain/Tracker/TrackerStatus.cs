namespace coordinator.Domain.Tracker
{
    public enum TrackerStatus
    {
        Initialise,
        RegisterDocumentIds,
        RegisterPdfBlobName,
        Complete,
        Error
    }
}
