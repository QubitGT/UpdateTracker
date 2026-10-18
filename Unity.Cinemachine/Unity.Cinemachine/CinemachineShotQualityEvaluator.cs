using System;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Shot Quality Evaluator")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineShotQualityEvaluator.html")]
public class CinemachineShotQualityEvaluator : CinemachineExtension, IShotQualityEvaluator
{
	[Serializable]
	public struct DistanceEvaluationSettings
	{
		[Tooltip("If enabled, will evaluate shot quality based on target distance")]
		public bool Enabled;

		[Tooltip("If greater than zero, maximum quality boost will occur when target is this far from the camera")]
		public float OptimalDistance;

		[Tooltip("Shots with targets closer to the camera than this will not get a quality boost")]
		public float NearLimit;

		[Tooltip("Shots with targets farther from the camera than this will not get a quality boost")]
		public float FarLimit;

		[Tooltip("High quality shots will be boosted by this fraction of their normal quality")]
		public float MaxQualityBoost;

		internal static DistanceEvaluationSettings Default
		{
			get
			{
				DistanceEvaluationSettings result = default(DistanceEvaluationSettings);
				result.NearLimit = 5f;
				result.FarLimit = 30f;
				result.OptimalDistance = 10f;
				result.MaxQualityBoost = 0.2f;
				return result;
			}
		}
	}

	[Tooltip("Objects on these layers will be detected")]
	public LayerMask OcclusionLayers = 1;

	[TagField]
	[Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
	public string IgnoreTag = string.Empty;

	[Tooltip("Obstacles closer to the target than this will be ignored")]
	public float MinimumDistanceFromTarget = 0.2f;

	[Tooltip("Radius of the spherecast that will be done to check for occlusions.")]
	public float CameraRadius;

	[FoldoutWithEnabledButton("Enabled")]
	public DistanceEvaluationSettings DistanceEvaluation = DistanceEvaluationSettings.Default;

	private void OnValidate()
	{
		CameraRadius = Mathf.Max(0f, CameraRadius);
		MinimumDistanceFromTarget = Mathf.Max(0.01f, MinimumDistanceFromTarget);
		CameraRadius = Mathf.Max(0f, CameraRadius);
		DistanceEvaluation.NearLimit = Mathf.Max(0.1f, DistanceEvaluation.NearLimit);
		DistanceEvaluation.FarLimit = Mathf.Max(DistanceEvaluation.NearLimit, DistanceEvaluation.FarLimit);
		DistanceEvaluation.OptimalDistance = Mathf.Clamp(DistanceEvaluation.OptimalDistance, DistanceEvaluation.NearLimit, DistanceEvaluation.FarLimit);
	}

	private void Reset()
	{
		OcclusionLayers = 1;
		IgnoreTag = string.Empty;
		MinimumDistanceFromTarget = 0.2f;
		CameraRadius = 0f;
		DistanceEvaluation = DistanceEvaluationSettings.Default;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != CinemachineCore.Stage.Finalize || !state.HasLookAt())
		{
			return;
		}
		if (state.IsTargetOffscreen() || IsTargetObscured(state))
		{
			state.ShotQuality *= 0.2f;
		}
		if (!DistanceEvaluation.Enabled)
		{
			return;
		}
		float num = 0f;
		if (!(DistanceEvaluation.OptimalDistance > 0f))
		{
			return;
		}
		float num2 = Vector3.Magnitude(state.ReferenceLookAt - state.GetFinalPosition());
		if (num2 <= DistanceEvaluation.OptimalDistance)
		{
			if (num2 >= DistanceEvaluation.NearLimit)
			{
				num = DistanceEvaluation.MaxQualityBoost * (num2 - DistanceEvaluation.NearLimit) / (DistanceEvaluation.OptimalDistance - DistanceEvaluation.NearLimit);
			}
		}
		else
		{
			num2 -= DistanceEvaluation.OptimalDistance;
			if (num2 < DistanceEvaluation.FarLimit)
			{
				num = DistanceEvaluation.MaxQualityBoost * (1f - num2 / DistanceEvaluation.FarLimit);
			}
		}
		state.ShotQuality *= 1f + num;
	}

	private bool IsTargetObscured(CameraState state)
	{
		Vector3 referenceLookAt = state.ReferenceLookAt;
		Vector3 correctedPosition = state.GetCorrectedPosition();
		Vector3 vector = referenceLookAt - correctedPosition;
		float magnitude = vector.magnitude;
		if (magnitude < Mathf.Max(MinimumDistanceFromTarget, 0.0001f))
		{
			return true;
		}
		RaycastHit hitInfo;
		return RuntimeUtility.SphereCastIgnoreTag(new Ray(correctedPosition, vector.normalized), CameraRadius, out hitInfo, magnitude - MinimumDistanceFromTarget, OcclusionLayers, in IgnoreTag);
	}
}
