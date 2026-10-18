using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public sealed class XRInteractionStrengthFilterDelegate : IXRInteractionStrengthFilter
{
	public Func<IXRInteractor, IXRInteractable, float, float> delegateToProcess { get; set; }

	public bool canProcess { get; set; } = true;


	public XRInteractionStrengthFilterDelegate(Func<IXRInteractor, IXRInteractable, float, float> delegateToProcess)
	{
		if (delegateToProcess == null)
		{
			throw new ArgumentException("delegateToProcess");
		}
		this.delegateToProcess = delegateToProcess;
	}

	public float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength)
	{
		return delegateToProcess(interactor, interactable, interactionStrength);
	}
}
