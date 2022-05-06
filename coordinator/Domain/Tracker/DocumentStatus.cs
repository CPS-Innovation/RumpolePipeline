namespace coordinator.Domain.Tracker
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		NotFoundInCDE,
		UnableToConvertToPdf,
		UnexpectedFailure
	}
}

