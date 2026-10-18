using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Decollider")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineDecollider.html")]
public class CinemachineDecollider : CinemachineExtension
{
	[Serializable]
	public struct DecollisionSettings
	{
		[Serializable]
		public struct FollowTargetSettings
		{
			[Tooltip("Use the Follow target when resolving occlusions, instead of the LookAt target.")]
			public bool Enabled;

			[Tooltip("Vertical offset from the Follow target's root, in target local space")]
			public float YOffset;
		}

		[Tooltip("When enabled, will attempt to push the camera out of intersecting objects")]
		public bool Enabled;

		[Tooltip("Objects on these layers will be detected")]
		public LayerMask ObstacleLayers;

		[EnabledProperty("Enabled", "")]
		public FollowTargetSettings UseFollowTarget;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera returns to its normal position after having been corrected.  Higher numbers will move the camera more gradually back to normal.")]
		public float Damping;

		[Range(0f, 2f)]
		[Tooltip("Smoothing to apply to obstruction resolution.  Nearest camera point is held for at least this long")]
		public float SmoothingTime;
	}

	[Serializable]
	public struct TerrainSettings
	{
		[Tooltip("When enabled, will attempt to place the camera on top of terrain layers")]
		public bool Enabled;

		[Tooltip("Colliders on these layers will be detected")]
		public LayerMask TerrainLayers;

		[Tooltip("Specifies the maximum length of a raycast used to find terrain colliders")]
		public float MaximumRaycast;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera returns to its normal position after having been corrected.  Higher numbers will move the camera more gradually back to normal.")]
		public float Damping;
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public float PreviousTerrainDisplacement;

		public float PreviousDistanceFromTarget;

		public Vector3 PreviouDecollisionDisplacement;

		public Vector3 PreviousCorrectedCameraPosition;

		private float m_SmoothedDistance;

		private float m_SmoothingStartTime;

		public float UpdateDistanceSmoothing(float distance, float smoothingTime, bool haveDisplacement)
		{
			if (haveDisplacement && (m_SmoothedDistance == 0f || distance <= m_SmoothedDistance))
			{
				m_SmoothedDistance = distance;
				m_SmoothingStartTime = CinemachineCore.CurrentTime;
			}
			if (m_SmoothingStartTime != 0f && CinemachineCore.CurrentTime - m_SmoothingStartTime < smoothingTime)
			{
				distance = Mathf.Min(distance, m_SmoothedDistance);
			}
			if (!haveDisplacement && CinemachineCore.CurrentTime - m_SmoothingStartTime >= smoothingTime)
			{
				m_SmoothedDistance = (m_SmoothingStartTime = 0f);
			}
			return distance;
		}
	}

	[Tooltip("Camera will try to maintain this distance from any obstacle or terrain.  Increase it if necessary to keep the camera from clipping the near edge of obsacles.")]
	public float CameraRadius = 0.4f;

	[FoldoutWithEnabledButton("Enabled")]
	public DecollisionSettings Decollision;

	[FoldoutWithEnabledButton("Enabled")]
	public TerrainSettings TerrainResolution;

	private const int kColliderBufferSize = 10;

	private static Collider[] s_ColliderBuffer = new Collider[10];

	private static float[] s_ColliderDistanceBuffer = new float[10];

	private static int[] s_ColliderOrderBuffer = new int[10];

	private static readonly IComparer<int> s_ColliderBufferSorter = Comparer<int>.Create(delegate(int a, int b)
	{
		if (s_ColliderDistanceBuffer[a] == s_ColliderDistanceBuffer[b])
		{
			return 0;
		}
		return (!(s_ColliderDistanceBuffer[a] > s_ColliderDistanceBuffer[b])) ? 1 : (-1);
	});

	private void OnValidate()
	{
		CameraRadius = Mathf.Max(0.01f, CameraRadius);
	}

	private void Reset()
	{
		CameraRadius = 0.4f;
		TerrainResolution = new TerrainSettings
		{
			Enabled = true,
			TerrainLayers = 1,
			MaximumRaycast = 10f,
			Damping = 0.5f
		};
		Decollision = new DecollisionSettings
		{
			Enabled = false,
			ObstacleLayers = 1,
			Damping = 0.5f
		};
	}

	protected override void OnDestroy()
	{
		RuntimeUtility.DestroyScratchCollider();
		base.OnDestroy();
	}

	public override float GetMaxDampTime()
	{
		return Mathf.Max(Decollision.Enabled ? Decollision.Damping : 0f, TerrainResolution.Enabled ? TerrainResolution.Damping : 0f);
	}

	public override void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
	{
		GetExtraState<VcamExtraState>(vcam).PreviousCorrectedCameraPosition = pos;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != 0)
		{
			return;
		}
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		Vector3 referenceUp = state.ReferenceUp;
		Vector3 correctedPosition = state.GetCorrectedPosition();
		bool num = state.HasLookAt();
		Vector3 vector = (num ? state.ReferenceLookAt : state.GetCorrectedPosition());
		Vector3 avoidanceResolutionTargetPoint = GetAvoidanceResolutionTargetPoint(vcam, ref state);
		Vector2 vector2 = (num ? state.RawOrientation.GetCameraRotationToTarget(vector - correctedPosition, state.ReferenceUp) : Vector2.zero);
		if (!vcam.PreviousStateIsValid)
		{
			deltaTime = -1f;
		}
		extraState.PreviousTerrainDisplacement = (TerrainResolution.Enabled ? ResolveTerrain(extraState, state.GetCorrectedPosition(), referenceUp, deltaTime) : 0f);
		state.PositionCorrection += extraState.PreviousTerrainDisplacement * referenceUp;
		if (Decollision.Enabled)
		{
			Vector3 correctedPosition2 = state.GetCorrectedPosition();
			Vector3 displacement = DecollideCamera(correctedPosition2, avoidanceResolutionTargetPoint);
			displacement = ApplySmoothingAndDamping(displacement, avoidanceResolutionTargetPoint, correctedPosition2, extraState, deltaTime);
			if (!displacement.AlmostZero())
			{
				state.PositionCorrection += displacement;
				float num2 = (TerrainResolution.Enabled ? ResolveTerrain(extraState, state.GetCorrectedPosition(), referenceUp, -1f) : 0f);
				if (Mathf.Abs(num2) > 0.0001f)
				{
					state.PositionCorrection += num2 * referenceUp;
					extraState.PreviousTerrainDisplacement = 0f;
				}
			}
		}
		Vector3 correctedPosition3 = state.GetCorrectedPosition();
		if (num && !(correctedPosition - correctedPosition3).AlmostZero())
		{
			Quaternion orient = Quaternion.LookRotation(vector - correctedPosition3, referenceUp);
			state.RawOrientation = orient.ApplyCameraRotation(-vector2, referenceUp);
			if (deltaTime >= 0f)
			{
				Vector3 v = extraState.PreviousCorrectedCameraPosition - vector;
				Vector3 v2 = correctedPosition3 - vector;
				if (v.sqrMagnitude > 0.0001f && v2.sqrMagnitude > 0.0001f)
				{
					state.RotationDampingBypass = UnityVectorExtensions.SafeFromToRotation(v, v2, referenceUp);
				}
			}
		}
		extraState.PreviousCorrectedCameraPosition = correctedPosition3;
	}

	private Vector3 GetAvoidanceResolutionTargetPoint(CinemachineVirtualCameraBase vcam, ref CameraState state)
	{
		Vector3 result = (state.HasLookAt() ? state.ReferenceLookAt : state.GetCorrectedPosition());
		if (Decollision.UseFollowTarget.Enabled)
		{
			Transform follow = vcam.Follow;
			if (follow != null)
			{
				result = TargetPositionCache.GetTargetPosition(follow) + TargetPositionCache.GetTargetRotation(follow) * Vector3.up * Decollision.UseFollowTarget.YOffset;
			}
		}
		return result;
	}

	private float ResolveTerrain(VcamExtraState extra, Vector3 camPos, Vector3 up, float deltaTime)
	{
		float num = 0f;
		if (RuntimeUtility.SphereCastIgnoreTag(new Ray(camPos + TerrainResolution.MaximumRaycast * up, -up), CameraRadius + 0.0001f, out var hitInfo, TerrainResolution.MaximumRaycast, TerrainResolution.TerrainLayers, in string.Empty))
		{
			num = TerrainResolution.MaximumRaycast - hitInfo.distance + 0.0001f;
		}
		if (deltaTime >= 0f && TerrainResolution.Damping > 0.0001f && num < extra.PreviousTerrainDisplacement)
		{
			num = extra.PreviousTerrainDisplacement + Damper.Damp(num - extra.PreviousTerrainDisplacement, TerrainResolution.Damping, deltaTime);
		}
		return num;
	}

	private Vector3 DecollideCamera(Vector3 cameraPos, Vector3 lookAtPoint)
	{
		LayerMask layerMask = Decollision.ObstacleLayers;
		if (TerrainResolution.Enabled)
		{
			layerMask = (int)layerMask & ~(int)TerrainResolution.TerrainLayers;
		}
		if ((int)layerMask == 0)
		{
			return Vector3.zero;
		}
		Vector3 vector = cameraPos - lookAtPoint;
		float magnitude = vector.magnitude;
		if (magnitude < 0.0001f)
		{
			return Vector3.zero;
		}
		int num = Physics.OverlapCapsuleNonAlloc(lookAtPoint, cameraPos, CameraRadius - 0.0001f, s_ColliderBuffer, layerMask, QueryTriggerInteraction.Ignore);
		if (num == 0)
		{
			return Vector3.zero;
		}
		vector /= magnitude;
		for (int i = 0; i < num; i++)
		{
			Collider obj = s_ColliderBuffer[i];
			s_ColliderOrderBuffer[i] = i;
			s_ColliderDistanceBuffer[i] = 0f;
			if (obj.Raycast(new Ray(lookAtPoint, vector), out var hitInfo, magnitude + CameraRadius))
			{
				float num2 = hitInfo.distance - CameraRadius;
				if (num2 < CameraRadius)
				{
					num2 = Mathf.Max(0.01f, num2 + (CameraRadius - num2) * 0.5f);
				}
				s_ColliderDistanceBuffer[i] = num2;
			}
		}
		Array.Sort(s_ColliderOrderBuffer, 0, num, s_ColliderBufferSorter);
		Vector3 vector2 = cameraPos;
		SphereCollider scratchCollider = RuntimeUtility.GetScratchCollider();
		scratchCollider.radius = CameraRadius - 0.0001f;
		for (int j = 0; j < num; j++)
		{
			int num3 = s_ColliderOrderBuffer[j];
			if (s_ColliderDistanceBuffer[num3] != 0f)
			{
				Collider collider = s_ColliderBuffer[num3];
				if (Physics.ComputePenetration(scratchCollider, vector2, Quaternion.identity, collider, collider.transform.position, collider.transform.rotation, out var _, out var _))
				{
					vector2 = lookAtPoint + vector * s_ColliderDistanceBuffer[num3];
				}
			}
		}
		return vector2 - cameraPos;
	}

	private Vector3 ApplySmoothingAndDamping(Vector3 displacement, Vector3 lookAtPoint, Vector3 oldCamPos, VcamExtraState extra, float deltaTime)
	{
		Vector3 vector = oldCamPos + displacement - lookAtPoint;
		float num = float.MaxValue;
		if (deltaTime >= 0f)
		{
			num = vector.magnitude;
			if (num > CameraRadius)
			{
				Vector3 vector2 = vector / num;
				if (Decollision.SmoothingTime > 0.0001f)
				{
					num = extra.UpdateDistanceSmoothing(num, Decollision.SmoothingTime, !displacement.AlmostZero());
					displacement = lookAtPoint + vector2 * num - oldCamPos;
				}
				if (Decollision.Damping > 0.0001f && num > extra.PreviousDistanceFromTarget)
				{
					float num2 = extra.PreviousDistanceFromTarget;
					float num3 = (oldCamPos - lookAtPoint).magnitude - extra.PreviouDecollisionDisplacement.magnitude;
					if (Mathf.Abs(num - num3) < Mathf.Abs(num - num2))
					{
						num2 = num3;
					}
					num = num2 + Damper.Damp(num - num2, Decollision.Damping, deltaTime);
					displacement = lookAtPoint + vector2 * num - oldCamPos;
				}
			}
		}
		extra.PreviousDistanceFromTarget = num;
		extra.PreviouDecollisionDisplacement = displacement;
		return displacement;
	}
}
