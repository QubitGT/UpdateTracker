using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class HoverExitEventArgs : BaseInteractionEventArgs
{
	public new IXRHoverInteractor interactorObject
	{
		get
		{
			return (IXRHoverInteractor)base.interactorObject;
		}
		set
		{
			base.interactorObject = value;
		}
	}

	public new IXRHoverInteractable interactableObject
	{
		get
		{
			return (IXRHoverInteractable)base.interactableObject;
		}
		set
		{
			base.interactableObject = value;
		}
	}

	public XRInteractionManager manager { get; set; }

	public bool isCanceled { get; set; }
}
