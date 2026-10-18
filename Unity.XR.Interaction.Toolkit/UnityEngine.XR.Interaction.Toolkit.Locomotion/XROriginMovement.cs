namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XROriginMovement : IXRBodyTransformation
{
	public Vector3 motion { get; set; }

	public bool forceUnconstrained { get; set; }

	public virtual void Apply(XRMovableBody body)
	{
		if (body.constrainedManipulator != null && !forceUnconstrained)
		{
			body.constrainedManipulator.MoveBody(motion);
		}
		else
		{
			body.originTransform.position += motion;
		}
	}
}
