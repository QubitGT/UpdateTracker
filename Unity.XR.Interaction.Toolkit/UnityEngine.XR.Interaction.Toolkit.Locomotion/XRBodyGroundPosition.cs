namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRBodyGroundPosition : IXRBodyTransformation
{
	public Vector3 targetPosition { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		Transform originTransform = body.originTransform;
		originTransform.position = targetPosition + originTransform.position - body.GetBodyGroundWorldPosition();
	}
}
