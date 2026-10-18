namespace Unity.Burst;

internal enum CompilationPriority
{
	EagerCompilationSynchronous,
	Asynchronous,
	ILPP,
	EagerCompilationAsynchronous
}
