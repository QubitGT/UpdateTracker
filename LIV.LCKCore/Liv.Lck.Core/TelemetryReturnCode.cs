namespace Liv.Lck.Core;

internal enum TelemetryReturnCode : uint
{
	Ok,
	Panic,
	FailedToClearContext,
	FailedToSetContext,
	FailedToRetrieveState,
	FailedToDeserializeContext,
	InvalidArgument
}
