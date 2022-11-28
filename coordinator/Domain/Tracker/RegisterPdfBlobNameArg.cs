namespace coordinator.Domain.Tracker
{
    public class RegisterPdfBlobNameArg
    {
        public string DocumentId { get; set; }
        
        public long VersionId { get; set; }

        public string BlobName { get; set; }
    }
}
