using System;
namespace coordinator.Domain.Tracker
{
	public enum TrackerStatus
	{
		NotStarted,
		Running,
		NoDocumentsFoundInCDE,
		Completed,
		Failed
	}
}

