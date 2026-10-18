using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

[CreateAssetMenu(fileName = "UnderCameraBodyPositionEvaluator", menuName = "XR/Locomotion/Under Camera Body Position Evaluator")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.UnderCameraBodyPositionEvaluator.html")]
public class UnderCameraBodyPositionEvaluator : ScriptableObject, IXRBodyPositionEvaluator
{
	public Vector3 GetBodyGroundLocalPosition(XROrigin xrOrigin)
	{
		Vector3 cameraInOriginSpacePos = xrOrigin.CameraInOriginSpacePos;
		cameraInOriginSpacePos.y = 0f;
		return cameraInOriginSpacePos;
	}
}
