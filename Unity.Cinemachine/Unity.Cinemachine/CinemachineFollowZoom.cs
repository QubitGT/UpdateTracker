using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Follow Zoom")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.LookAt)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineFollowZoom.html")]
public class CinemachineFollowZoom : CinemachineExtension
{
	private class VcamExtraState : VcamExtraStateBase
	{
		public float m_PreviousFrameZoom;
	}

	[Tooltip("The shot width to maintain, in world units, at target distance.")]
	[FormerlySerializedAs("m_Width")]
	public float Width = 2f;

	[Range(0f, 20f)]
	[Tooltip("Increase this value to soften the aggressiveness of the follow-zoom.  Small numbers are more responsive, larger numbers give a more heavy slowly responding camera.")]
	[FormerlySerializedAs("m_Damping")]
	public float Damping = 1f;

	[MinMaxRangeSlider(1f, 179f)]
	[Tooltip("Range for the FOV that this behaviour will generate.")]
	public Vector2 FovRange = new Vector2(3f, 60f);

	private void Reset()
	{
		Width = 2f;
		Damping = 1f;
		FovRange = new Vector2(3f, 60f);
	}

	private void OnValidate()
	{
		Width = Mathf.Max(0f, Width);
		FovRange.y = Mathf.Clamp(FovRange.y, 1f, 179f);
		FovRange.x = Mathf.Clamp(FovRange.x, 1f, FovRange.y);
	}

	public override float GetMaxDampTime()
	{
		return Damping;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		if (deltaTime < 0f || !vcam.PreviousStateIsValid)
		{
			extraState.m_PreviousFrameZoom = state.Lens.FieldOfView;
		}
		if (stage != 0)
		{
			return;
		}
		float value = Mathf.Max(Width, 0f);
		float value2 = 179f;
		float num = Vector3.Distance(state.GetCorrectedPosition(), state.ReferenceLookAt);
		if (num > 0.0001f)
		{
			float min = num * 2f * Mathf.Tan(FovRange.x * (MathF.PI / 180f) / 2f);
			float max = num * 2f * Mathf.Tan(FovRange.y * (MathF.PI / 180f) / 2f);
			value = Mathf.Clamp(value, min, max);
			if (deltaTime >= 0f && Damping > 0f && vcam.PreviousStateIsValid)
			{
				float num2 = num * 2f * Mathf.Tan(extraState.m_PreviousFrameZoom * (MathF.PI / 180f) / 2f);
				float initial = value - num2;
				initial = vcam.DetachedLookAtTargetDamp(initial, Damping, deltaTime);
				value = num2 + initial;
			}
			value2 = 2f * Mathf.Atan(value / (2f * num)) * 57.29578f;
		}
		LensSettings lens = state.Lens;
		lens.FieldOfView = (extraState.m_PreviousFrameZoom = Mathf.Clamp(value2, FovRange.x, FovRange.y));
		state.Lens = lens;
	}
}
