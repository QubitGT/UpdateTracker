namespace PlayFab.ExperimentationModels;

public enum AnalysisTaskState
{
	Waiting,
	ReadyForSubmission,
	SubmittingToPipeline,
	Running,
	Completed,
	Failed,
	Canceled
}
