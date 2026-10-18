using Liv.Lck;

internal interface ILckCaptureStateProvider
{
	LckCaptureState CurrentCaptureState { get; }

	LckResult<bool> IsPaused();
}
