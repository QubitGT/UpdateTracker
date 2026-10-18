using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class DeactivateEventArgs : BaseInteractionEventArgs
{
	public new IXRActivateInteractor interactorObject
	{
		get
		{
			return (IXRActivateInteractor)base.interactorObject;
		}
		set
		{
			base.interactorObject = value;
		}
	}

	public new IXRActivateInteractable interactableObject
	{
		get
		{
			return (IXRActivateInteractable)base.interactableObject;
		}
		set
		{
			base.interactableObject = value;
		}
	}
}
