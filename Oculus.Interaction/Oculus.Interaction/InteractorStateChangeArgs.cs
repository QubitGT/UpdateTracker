namespace Oculus.Interaction;

public struct InteractorStateChangeArgs
{
	public InteractorState PreviousState { get; }

	public InteractorState NewState { get; }

	public InteractorStateChangeArgs(InteractorState previousState, InteractorState newState)
	{
		PreviousState = previousState;
		NewState = newState;
	}
}
