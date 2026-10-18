using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public interface IXRTargetEvaluatorLinkable
{
	void OnLink(IXRInteractor interactor);

	void OnUnlink(IXRInteractor interactor);
}
