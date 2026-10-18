using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRCameraForwardXZAlignment : IXRBodyTransformation
{
	public Vector3 targetDirection { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		XROrigin xrOrigin = body.xrOrigin;
		Vector3 up = body.originTransform.up;
		Vector3 normalized = Vector3.ProjectOnPlane(xrOrigin.Camera.transform.forward, up).normalized;
		Vector3 normalized2 = Vector3.ProjectOnPlane(targetDirection, up).normalized;
		float angleDegrees = Vector3.SignedAngle(normalized, normalized2, up);
		xrOrigin.RotateAroundCameraPosition(up, angleDegrees);
	}
}
