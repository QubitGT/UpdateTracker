using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit;

public class InteractableUnregisteredEventArgs : BaseRegistrationEventArgs
{
	public IXRInteractable interactableObject { get; set; }

	[Obsolete("interactable has been deprecated. Use interactableObject instead.", true)]
	public XRBaseInteractable interactable
	{
		get
		{
			return null;
		}
		set
		{
		}
	}
}
