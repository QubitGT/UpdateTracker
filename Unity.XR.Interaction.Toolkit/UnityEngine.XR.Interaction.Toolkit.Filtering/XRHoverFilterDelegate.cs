using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public sealed class XRHoverFilterDelegate : IXRHoverFilter
{
	public Func<IXRHoverInteractor, IXRHoverInteractable, bool> delegateToProcess { get; set; }

	public bool canProcess { get; set; } = true;


	public XRHoverFilterDelegate(Func<IXRHoverInteractor, IXRHoverInteractable, bool> delegateToProcess)
	{
		if (delegateToProcess == null)
		{
			throw new ArgumentException("delegateToProcess");
		}
		this.delegateToProcess = delegateToProcess;
	}

	public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		return delegateToProcess(interactor, interactable);
	}
}
