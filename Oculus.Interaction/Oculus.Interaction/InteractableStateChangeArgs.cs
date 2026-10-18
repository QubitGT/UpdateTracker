namespace Oculus.Interaction;

public struct InteractableStateChangeArgs
{
	public InteractableState PreviousState { get; }

	public InteractableState NewState { get; }

	public InteractableStateChangeArgs(InteractableState previousState, InteractableState newState)
	{
		PreviousState = previousState;
		NewState = newState;
	}
}
