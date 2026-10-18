namespace Oculus.Interaction;

public interface IInteractable : IInteractableView
{
	new int MaxInteractors { get; set; }

	new int MaxSelectingInteractors { get; set; }

	void Enable();

	void Disable();

	void RemoveInteractorByIdentifier(int id);
}
