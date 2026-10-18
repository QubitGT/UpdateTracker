using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class XRMovableBody
{
	public XROrigin xrOrigin { get; private set; }

	public Transform originTransform => xrOrigin.Origin.transform;

	public IXRBodyPositionEvaluator bodyPositionEvaluator { get; private set; }

	public IConstrainedXRBodyManipulator constrainedManipulator { get; private set; }

	public XRMovableBody(XROrigin xrOrigin, IXRBodyPositionEvaluator bodyPositionEvaluator)
	{
		this.xrOrigin = xrOrigin;
		this.bodyPositionEvaluator = bodyPositionEvaluator;
	}

	public Vector3 GetBodyGroundLocalPosition()
	{
		return bodyPositionEvaluator.GetBodyGroundLocalPosition(xrOrigin);
	}

	public Vector3 GetBodyGroundWorldPosition()
	{
		return bodyPositionEvaluator.GetBodyGroundWorldPosition(xrOrigin);
	}

	public void LinkConstrainedManipulator(IConstrainedXRBodyManipulator manipulator)
	{
		constrainedManipulator?.OnUnlinkedFromBody();
		manipulator.linkedBody?.UnlinkConstrainedManipulator();
		constrainedManipulator = manipulator;
		constrainedManipulator.OnLinkedToBody(this);
	}

	public void UnlinkConstrainedManipulator()
	{
		constrainedManipulator?.OnUnlinkedFromBody();
		constrainedManipulator = null;
	}
}
