using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Pixel Perfect")]
[ExecuteAlways]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePixelPerfect.html")]
public class CinemachinePixelPerfect : CinemachineExtension
{
	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != 0)
		{
			return;
		}
		CinemachineBrain cinemachineBrain = CinemachineCore.FindPotentialTargetBrain(vcam);
		if (!(cinemachineBrain == null) && cinemachineBrain.IsLiveChild(vcam))
		{
			cinemachineBrain.TryGetComponent<PixelPerfectCamera>(out var component);
			if (!(component == null) && component.isActiveAndEnabled)
			{
				LensSettings lens = state.Lens;
				lens.OrthographicSize = component.CorrectCinemachineOrthoSize(lens.OrthographicSize);
				state.Lens = lens;
			}
		}
	}
}
