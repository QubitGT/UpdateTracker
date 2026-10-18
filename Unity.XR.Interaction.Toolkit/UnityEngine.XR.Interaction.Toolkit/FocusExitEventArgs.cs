using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class FocusExitEventArgs : BaseInteractionEventArgs
{
	public IXRInteractionGroup interactionGroup { get; set; }

	public new IXRFocusInteractable interactableObject
	{
		get
		{
			return (IXRFocusInteractable)base.interactableObject;
		}
		set
		{
			base.interactableObject = value;
		}
	}

	public XRInteractionManager manager { get; set; }

	public bool isCanceled { get; set; }
}
