namespace coordinator.Domain.Tracker
{
	public enum TrackerStatus
	{
		Initialised,
		NotStarted,
		Running,
		NoDocumentsFoundInCDE,
		Completed,
		Failed,
		UnableToEvaluateExistingDocuments
	}
}

