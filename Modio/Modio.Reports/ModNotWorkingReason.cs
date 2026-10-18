namespace Modio.Reports;

public enum ModNotWorkingReason
{
	None,
	CrashesGame,
	DoesNotLoad,
	ConflictsWithOtherMods,
	MissingDependencies,
	InstallationIssues,
	BuggyBehaviour,
	IncompatibleWithGameVersion,
	FileCorruption
}
