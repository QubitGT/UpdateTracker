using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public abstract class BaseInteractionEventArgs
{
	public IXRInteractor interactorObject { get; set; }

	public IXRInteractable interactableObject { get; set; }

	[Obsolete("interactor has been deprecated. Use interactorObject instead.", true)]
	public XRBaseInteractor interactor
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

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
