namespace coordinator.Domain.Tracker
{
	public enum TrackerStatus
	{
		NotStarted,
		Running,
		NoDocumentsFoundInCde,
		Completed,
		Failed,
		UnableToEvaluateExistingDocuments
	}
}

