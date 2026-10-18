using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

public interface IXRGrabTransformer
{
	bool canProcess { get; }

	void OnLink(XRGrabInteractable grabInteractable);

	void OnGrab(XRGrabInteractable grabInteractable);

	void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale);

	void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale);

	void OnUnlink(XRGrabInteractable grabInteractable);
}
