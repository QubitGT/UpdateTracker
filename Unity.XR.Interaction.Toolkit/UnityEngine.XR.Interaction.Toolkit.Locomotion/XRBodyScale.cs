namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRBodyScale : IXRBodyTransformation
{
	public float uniformScale { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		Vector3 bodyGroundWorldPosition = body.GetBodyGroundWorldPosition();
		Transform originTransform = body.originTransform;
		originTransform.localScale = Vector3.one * uniformScale;
		Vector3 bodyGroundWorldPosition2 = body.GetBodyGroundWorldPosition();
		originTransform.position = bodyGroundWorldPosition + originTransform.position - bodyGroundWorldPosition2;
	}
}
