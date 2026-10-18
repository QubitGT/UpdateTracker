using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public interface IXRBodyPositionEvaluator
{
	Vector3 GetBodyGroundLocalPosition(XROrigin xrOrigin);
}
