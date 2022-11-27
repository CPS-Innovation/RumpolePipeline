namespace coordinator.Domain.Tracker
{
	public enum DocumentStatus
	{
		None,
		PdfUploadedToBlob,
		Indexed,
		NotFoundInDDEI,
		UnableToConvertToPdf,
		UnexpectedFailure,
		OcrAndIndexFailure,
		DocumentAlreadyProcessed,
		DocumentEvaluated,
		DocumentRemovedFromSearchIndex,
		UnexpectedSearchIndexRemovalFailure,
		SearchIndexUpdateFailure,
		UnableToEvaluateDocument
	}
}

