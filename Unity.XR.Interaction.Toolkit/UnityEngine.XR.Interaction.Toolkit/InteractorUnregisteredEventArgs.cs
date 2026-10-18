using System;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public class InteractorUnregisteredEventArgs : BaseRegistrationEventArgs
{
	public IXRInteractor interactorObject { get; set; }

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
}
