namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XROriginUpAlignment : IXRBodyTransformation
{
	public Vector3 targetUp { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		body.xrOrigin.MatchOriginUp(targetUp);
	}
}
