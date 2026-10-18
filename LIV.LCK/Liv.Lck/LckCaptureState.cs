namespace Liv.Lck;

internal enum LckCaptureState
{
	Idle,
	Starting,
	InProgress,
	Paused,
	Stopping,
	Blocked
}
