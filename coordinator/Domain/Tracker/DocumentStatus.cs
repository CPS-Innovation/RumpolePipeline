namespace coordinator.Domain.Tracker
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		Indexed,
		NotFoundInCDE,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure
	}
}

