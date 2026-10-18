namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRBodyYawRotation : IXRBodyTransformation
{
	public float angleDelta { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		Transform originTransform = body.originTransform;
		originTransform.RotateAround(body.GetBodyGroundWorldPosition(), originTransform.up, angleDelta);
	}
}
