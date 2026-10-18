using System;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

[AddComponentMenu("")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRLegacyGrabTransformer.html")]
[Obsolete("XRLegacyGrabTransformer has been deprecated, use XRSingleFreeGrabTransformer instead.", true)]
public sealed class XRLegacyGrabTransformer : XRBaseGrabTransformer
{
	public override void OnLink(XRGrabInteractable grabInteractable)
	{
		base.OnLink(grabInteractable);
	}

	public override void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
	{
	}

	public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
	}
}
