using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public sealed class XRSelectFilterDelegate : IXRSelectFilter
{
	public Func<IXRSelectInteractor, IXRSelectInteractable, bool> delegateToProcess { get; set; }

	public bool canProcess { get; set; } = true;


	public XRSelectFilterDelegate(Func<IXRSelectInteractor, IXRSelectInteractable, bool> delegateToProcess)
	{
		if (delegateToProcess == null)
		{
			throw new ArgumentException("delegateToProcess");
		}
		this.delegateToProcess = delegateToProcess;
	}

	public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		return delegateToProcess(interactor, interactable);
	}
}
