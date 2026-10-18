using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public static class XRBodyPositionEvaluatorExtensions
{
	public static Vector3 GetBodyGroundWorldPosition(this IXRBodyPositionEvaluator evaluator, XROrigin xrOrigin)
	{
		return xrOrigin.Origin.transform.TransformPoint(evaluator.GetBodyGroundLocalPosition(xrOrigin));
	}
}
