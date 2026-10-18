using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public abstract class XRBaseTargetFilter : MonoBehaviour, IXRTargetFilter
{
	public virtual bool canProcess => base.isActiveAndEnabled;

	public virtual void Link(IXRInteractor interactor)
	{
	}

	public virtual void Unlink(IXRInteractor interactor)
	{
	}

	public abstract void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results);
}
