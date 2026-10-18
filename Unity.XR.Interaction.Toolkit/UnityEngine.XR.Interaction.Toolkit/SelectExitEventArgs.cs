using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class SelectExitEventArgs : BaseInteractionEventArgs
{
	public new IXRSelectInteractor interactorObject
	{
		get
		{
			return (IXRSelectInteractor)base.interactorObject;
		}
		set
		{
			base.interactorObject = value;
		}
	}

	public new IXRSelectInteractable interactableObject
	{
		get
		{
			return (IXRSelectInteractable)base.interactableObject;
		}
		set
		{
			base.interactableObject = value;
		}
	}

	public XRInteractionManager manager { get; set; }

	public bool isCanceled { get; set; }
}
