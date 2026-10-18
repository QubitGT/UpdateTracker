using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Deoccluder")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineDeoccluder.html")]
public class CinemachineDeoccluder : CinemachineExtension, IShotQualityEvaluator
{
	[Serializable]
	public struct ObstacleAvoidance
	{
		[Serializable]
		public struct FollowTargetSettings
		{
			[Tooltip("Use the Follow target when resolving occlusions, instead of the LookAt target.")]
			public bool Enabled;

			[Tooltip("Vertical offset from the Follow target's root, in target local space")]
			public float YOffset;
		}

		public enum ResolutionStrategy
		{
			PullCameraForward,
			PreserveCameraHeight,
			PreserveCameraDistance
		}

		[Tooltip("When enabled, will attempt to resolve situations where the line of sight to the target is blocked by an obstacle")]
		public bool Enabled;

		[Tooltip("The maximum raycast distance when checking if the line of sight to this camera's target is clear.  If the setting is 0 or less, the current actual distance to target will be used.")]
		public float DistanceLimit;

		[Tooltip("Don't take action unless occlusion has lasted at least this long.")]
		public float MinimumOcclusionTime;

		[Tooltip("Camera will try to maintain this distance from any obstacle.  Try to keep this value small.  Increase it if you are seeing inside obstacles due to a large FOV on the camera.")]
		public float CameraRadius;

		[EnabledProperty("Enabled", "")]
		public FollowTargetSettings UseFollowTarget;

		[Tooltip("The way in which the Deoccluder will attempt to preserve sight of the target.")]
		public ResolutionStrategy Strategy;

		[Range(1f, 10f)]
		[Tooltip("Upper limit on how many obstacle hits to process.  Higher numbers may impact performance.  In most environments, 4 is enough.")]
		public int MaximumEffort;

		[Range(0f, 2f)]
		[Tooltip("Smoothing to apply to obstruction resolution.  Nearest camera point is held for at least this long")]
		public float SmoothingTime;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera returns to its normal position after having been corrected.  Higher numbers will move the camera more gradually back to normal.")]
		public float Damping;

		[Range(0f, 10f)]
		[Tooltip("How gradually the camera moves to resolve an occlusion.  Higher numbers will move the camera more gradually.")]
		public float DampingWhenOccluded;

		internal static ObstacleAvoidance Default
		{
			get
			{
				ObstacleAvoidance result = default(ObstacleAvoidance);
				result.Enabled = true;
				result.DistanceLimit = 0f;
				result.MinimumOcclusionTime = 0f;
				result.CameraRadius = 0.4f;
				result.Strategy = ResolutionStrategy.PullCameraForward;
				result.MaximumEffort = 4;
				result.SmoothingTime = 0f;
				result.Damping = 0.4f;
				result.DampingWhenOccluded = 0.2f;
				return result;
			}
		}
	}

	[Serializable]
	public struct QualityEvaluation
	{
		[Tooltip("If enabled, will evaluate shot quality based on target distance and occlusion")]
		public bool Enabled;

		[Tooltip("If greater than zero, maximum quality boost will occur when target is this far from the camera")]
		public float OptimalDistance;

		[Tooltip("Shots with targets closer to the camera than this will not get a quality boost")]
		public float NearLimit;

		[Tooltip("Shots with targets farther from the camera than this will not get a quality boost")]
		public float FarLimit;

		[Tooltip("High quality shots will be boosted by this fraction of their normal quality")]
		public float MaxQualityBoost;

		internal static QualityEvaluation Default
		{
			get
			{
				QualityEvaluation result = default(QualityEvaluation);
				result.NearLimit = 5f;
				result.FarLimit = 30f;
				result.OptimalDistance = 10f;
				result.MaxQualityBoost = 0.2f;
				return result;
			}
		}
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public Vector3 PreviousDisplacement;

		public bool TargetObscured;

		public float OcclusionStartTime;

		public List<Vector3> DebugResolutionPath;

		public List<Collider> OccludingObjects;

		public Vector3 PreviousCameraOffset;

		public Vector3 PreviousCameraPosition;

		public float PreviousDampTime;

		public bool StateIsValid;

		private float m_SmoothedDistance;

		private float m_SmoothedTime;

		public void AddPointToDebugPath(Vector3 p, Collider c)
		{
		}

		public float ApplyDistanceSmoothing(float distance, float smoothingTime)
		{
			if (m_SmoothedTime != 0f && smoothingTime > 0.0001f && CinemachineCore.CurrentTime - m_SmoothedTime < smoothingTime)
			{
				return Mathf.Min(distance, m_SmoothedDistance);
			}
			return distance;
		}

		public void UpdateDistanceSmoothing(float distance)
		{
			if (!StateIsValid || m_SmoothedDistance == 0f || distance < m_SmoothedDistance)
			{
				m_SmoothedDistance = distance;
				m_SmoothedTime = CinemachineCore.CurrentTime;
			}
		}

		public void ResetDistanceSmoothing(float smoothingTime)
		{
			if (CinemachineCore.CurrentTime - m_SmoothedTime >= smoothingTime)
			{
				m_SmoothedDistance = (m_SmoothedTime = 0f);
			}
		}
	}

	[Tooltip("Objects on these layers will be detected")]
	public LayerMask CollideAgainst = 1;

	[TagField]
	[Tooltip("Obstacles with this tag will be ignored.  It is a good idea to set this field to the target's tag")]
	public string IgnoreTag = string.Empty;

	[Tooltip("Objects on these layers will never obstruct view of the target")]
	public LayerMask TransparentLayers = 0;

	[Tooltip("Obstacles closer to the target than this will be ignored")]
	public float MinimumDistanceFromTarget = 0.3f;

	[FoldoutWithEnabledButton("Enabled")]
	public ObstacleAvoidance AvoidObstacles;

	[FoldoutWithEnabledButton("Enabled")]
	public QualityEvaluation ShotQualityEvaluation = QualityEvaluation.Default;

	private List<VcamExtraState> m_extraStateCache;

	private const float k_PrecisionSlush = 0.001f;

	private RaycastHit[] m_CornerBuffer = new RaycastHit[4];

	private const float k_AngleThreshold = 0.1f;

	private static Collider[] s_ColliderBuffer = new Collider[5];

	public bool IsTargetObscured(CinemachineVirtualCameraBase vcam)
	{
		return GetExtraState<VcamExtraState>(vcam).TargetObscured;
	}

	public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
	{
		return GetCameraDisplacementDistance(vcam) > 0f;
	}

	public float GetCameraDisplacementDistance(CinemachineVirtualCameraBase vcam)
	{
		return GetExtraState<VcamExtraState>(vcam).PreviousDisplacement.magnitude;
	}

	private void OnValidate()
	{
		AvoidObstacles.DistanceLimit = Mathf.Max(0f, AvoidObstacles.DistanceLimit);
		AvoidObstacles.MinimumOcclusionTime = Mathf.Max(0f, AvoidObstacles.MinimumOcclusionTime);
		AvoidObstacles.CameraRadius = Mathf.Max(0f, AvoidObstacles.CameraRadius);
		MinimumDistanceFromTarget = Mathf.Max(0.01f, MinimumDistanceFromTarget);
		ShotQualityEvaluation.NearLimit = Mathf.Max(0.1f, ShotQualityEvaluation.NearLimit);
		ShotQualityEvaluation.FarLimit = Mathf.Max(ShotQualityEvaluation.NearLimit, ShotQualityEvaluation.FarLimit);
		ShotQualityEvaluation.OptimalDistance = Mathf.Clamp(ShotQualityEvaluation.OptimalDistance, ShotQualityEvaluation.NearLimit, ShotQualityEvaluation.FarLimit);
	}

	private void Reset()
	{
		CollideAgainst = 1;
		IgnoreTag = string.Empty;
		TransparentLayers = 0;
		MinimumDistanceFromTarget = 0.3f;
		AvoidObstacles = ObstacleAvoidance.Default;
		ShotQualityEvaluation = QualityEvaluation.Default;
	}

	protected override void OnDestroy()
	{
		RuntimeUtility.DestroyScratchCollider();
		base.OnDestroy();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		List<VcamExtraState> list = new List<VcamExtraState>();
		GetAllExtraStates(list);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].StateIsValid = false;
		}
	}

	public void DebugCollisionPaths(List<List<Vector3>> paths, List<List<Collider>> obstacles)
	{
		paths?.Clear();
		obstacles?.Clear();
		if (m_extraStateCache == null)
		{
			m_extraStateCache = new List<VcamExtraState>();
		}
		GetAllExtraStates(m_extraStateCache);
		for (int i = 0; i < m_extraStateCache.Count; i++)
		{
			VcamExtraState vcamExtraState = m_extraStateCache[i];
			if (vcamExtraState.DebugResolutionPath != null && vcamExtraState.DebugResolutionPath.Count > 0)
			{
				paths?.Add(vcamExtraState.DebugResolutionPath);
				obstacles?.Add(vcamExtraState.OccludingObjects);
			}
		}
	}

	public override float GetMaxDampTime()
	{
		if (!AvoidObstacles.Enabled)
		{
			return 0f;
		}
		return Mathf.Max(AvoidObstacles.Damping, Mathf.Max(AvoidObstacles.DampingWhenOccluded, AvoidObstacles.SmoothingTime));
	}

	public override void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
	{
		GetExtraState<VcamExtraState>(vcam).PreviousCameraPosition += positionDelta;
	}

	public override void ForceCameraPosition(CinemachineVirtualCameraBase vcam, Vector3 pos, Quaternion rot)
	{
		GetExtraState<VcamExtraState>(vcam).PreviousCameraPosition = pos;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage == CinemachineCore.Stage.Body)
		{
			VcamExtraState extra = GetExtraState<VcamExtraState>(vcam);
			extra.TargetObscured = false;
			extra.DebugResolutionPath?.Clear();
			extra.OccludingObjects?.Clear();
			if (!vcam.PreviousStateIsValid || deltaTime < 0f)
			{
				extra.StateIsValid = false;
			}
			if (!AvoidObstacles.Enabled)
			{
				extra.StateIsValid = false;
			}
			else
			{
				Vector3 correctedPosition = state.GetCorrectedPosition();
				Vector3 referenceUp = state.ReferenceUp;
				bool num = state.HasLookAt();
				Vector3 vector = (num ? state.ReferenceLookAt : state.GetCorrectedPosition());
				Vector3 resolutuionTargetPoint;
				bool avoidanceResolutionTargetPoint = GetAvoidanceResolutionTargetPoint(vcam, ref state, out resolutuionTargetPoint);
				Vector2 vector2 = (num ? state.RawOrientation.GetCameraRotationToTarget(vector - correctedPosition, referenceUp) : Vector2.zero);
				Quaternion rotationDampingBypass = state.RotationDampingBypass;
				if (extra.StateIsValid)
				{
					extra.PreviousDisplacement = rotationDampingBypass * extra.PreviousDisplacement;
				}
				Vector3 vector3 = (avoidanceResolutionTargetPoint ? PreserveLineOfSight(ref state, ref extra, resolutuionTargetPoint) : Vector3.zero);
				if (AvoidObstacles.MinimumOcclusionTime > 0.0001f)
				{
					float currentTime = CinemachineCore.CurrentTime;
					if (vector3.AlmostZero())
					{
						extra.OcclusionStartTime = 0f;
					}
					else
					{
						if (extra.OcclusionStartTime <= 0f)
						{
							extra.OcclusionStartTime = currentTime;
						}
						if (extra.StateIsValid && currentTime - extra.OcclusionStartTime < AvoidObstacles.MinimumOcclusionTime)
						{
							vector3 = extra.PreviousDisplacement;
						}
					}
				}
				if (avoidanceResolutionTargetPoint && AvoidObstacles.SmoothingTime > 0.0001f)
				{
					if (!extra.StateIsValid)
					{
						extra.ResetDistanceSmoothing(0f);
					}
					Vector3 vector4 = correctedPosition + vector3;
					Vector3 vector5 = vector4 - resolutuionTargetPoint;
					float magnitude = vector5.magnitude;
					if (magnitude > 0.0001f)
					{
						vector5 /= magnitude;
						if (!vector3.AlmostZero())
						{
							extra.UpdateDistanceSmoothing(magnitude);
						}
						magnitude = extra.ApplyDistanceSmoothing(magnitude, AvoidObstacles.SmoothingTime);
						vector3 += resolutuionTargetPoint + vector5 * magnitude - vector4;
					}
				}
				if (vector3.AlmostZero())
				{
					extra.ResetDistanceSmoothing(AvoidObstacles.SmoothingTime);
				}
				Vector3 cameraPos = correctedPosition + vector3;
				if (AvoidObstacles.Strategy != 0)
				{
					vector3 += RespectCameraRadius(cameraPos, resolutuionTargetPoint);
				}
				float num2 = AvoidObstacles.DampingWhenOccluded;
				if (avoidanceResolutionTargetPoint && extra.StateIsValid && AvoidObstacles.DampingWhenOccluded + AvoidObstacles.Damping > 0.0001f)
				{
					float sqrMagnitude = vector3.sqrMagnitude;
					float sqrMagnitude2 = extra.PreviousDisplacement.sqrMagnitude;
					if (Mathf.Abs(sqrMagnitude - sqrMagnitude2) > 9.999999E-09f)
					{
						num2 = ((sqrMagnitude > sqrMagnitude2) ? AvoidObstacles.DampingWhenOccluded : AvoidObstacles.Damping);
						if (sqrMagnitude < 0.0001f && num2 < extra.PreviousDampTime)
						{
							num2 = extra.PreviousDampTime + Damper.Damp(num2 - extra.PreviousDampTime, num2, deltaTime);
						}
						if (AvoidObstacles.Strategy == ObstacleAvoidance.ResolutionStrategy.PullCameraForward)
						{
							Vector3 vector6 = correctedPosition + vector3 - resolutuionTargetPoint;
							float magnitude2 = vector6.magnitude;
							Vector3 vector7 = vector6 / magnitude2;
							float num3 = extra.PreviousCameraOffset.magnitude;
							float num4 = (correctedPosition - resolutuionTargetPoint).magnitude - Mathf.Sqrt(sqrMagnitude2);
							if (Mathf.Abs(magnitude2 - num4) < Mathf.Abs(magnitude2 - num3))
							{
								num3 = num4;
							}
							magnitude2 = num3 + Damper.Damp(magnitude2 - num3, num2, deltaTime);
							cameraPos = resolutuionTargetPoint + vector7 * magnitude2;
							vector3 = cameraPos - correctedPosition;
						}
						else
						{
							Vector3 vector8 = resolutuionTargetPoint + rotationDampingBypass * extra.PreviousCameraOffset - correctedPosition;
							if (vector8.sqrMagnitude > sqrMagnitude2)
							{
								vector8 = extra.PreviousDisplacement;
							}
							vector3 = vector8 + Damper.Damp(vector3 - vector8, num2, deltaTime);
						}
					}
				}
				state.PositionCorrection += vector3;
				cameraPos = state.GetCorrectedPosition();
				if (num && vector3.sqrMagnitude > 0.0001f)
				{
					Quaternion orient = Quaternion.LookRotation(vector - cameraPos, referenceUp);
					state.RawOrientation = orient.ApplyCameraRotation(-vector2, referenceUp);
					if (extra.StateIsValid)
					{
						Vector3 v = extra.PreviousCameraPosition - vector;
						Vector3 v2 = cameraPos - vector;
						if (v.sqrMagnitude > 0.0001f && v2.sqrMagnitude > 0.0001f)
						{
							state.RotationDampingBypass = UnityVectorExtensions.SafeFromToRotation(v, v2, referenceUp);
						}
					}
				}
				extra.PreviousDisplacement = vector3;
				extra.PreviousCameraOffset = cameraPos - resolutuionTargetPoint;
				extra.PreviousCameraPosition = cameraPos;
				extra.PreviousDampTime = num2;
				extra.StateIsValid = true;
			}
		}
		if (stage != CinemachineCore.Stage.Finalize || !ShotQualityEvaluation.Enabled || !state.HasLookAt())
		{
			return;
		}
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		extraState.TargetObscured = state.IsTargetOffscreen() || IsTargetObscured(state);
		if (extraState.TargetObscured)
		{
			state.ShotQuality *= 0.2f;
		}
		if (extraState.StateIsValid && !extraState.PreviousDisplacement.AlmostZero())
		{
			state.ShotQuality *= 0.8f;
		}
		float num5 = 0f;
		if (!(ShotQualityEvaluation.OptimalDistance > 0f))
		{
			return;
		}
		float num6 = Vector3.Magnitude(state.ReferenceLookAt - state.GetFinalPosition());
		if (num6 <= ShotQualityEvaluation.OptimalDistance)
		{
			if (num6 >= ShotQualityEvaluation.NearLimit)
			{
				num5 = ShotQualityEvaluation.MaxQualityBoost * (num6 - ShotQualityEvaluation.NearLimit) / (ShotQualityEvaluation.OptimalDistance - ShotQualityEvaluation.NearLimit);
			}
		}
		else
		{
			num6 -= ShotQualityEvaluation.OptimalDistance;
			if (num6 < ShotQualityEvaluation.FarLimit)
			{
				num5 = ShotQualityEvaluation.MaxQualityBoost * (1f - num6 / ShotQualityEvaluation.FarLimit);
			}
		}
		state.ShotQuality *= 1f + num5;
	}

	private bool GetAvoidanceResolutionTargetPoint(CinemachineVirtualCameraBase vcam, ref CameraState state, out Vector3 resolutuionTargetPoint)
	{
		bool flag = state.HasLookAt();
		resolutuionTargetPoint = (flag ? state.ReferenceLookAt : state.GetCorrectedPosition());
		if (AvoidObstacles.UseFollowTarget.Enabled)
		{
			Transform follow = vcam.Follow;
			if (follow != null)
			{
				flag = true;
				resolutuionTargetPoint = TargetPositionCache.GetTargetPosition(follow) + TargetPositionCache.GetTargetRotation(follow) * Vector3.up * AvoidObstacles.UseFollowTarget.YOffset;
			}
		}
		return flag;
	}

	private Vector3 PreserveLineOfSight(ref CameraState state, ref VcamExtraState extra, Vector3 lookAtPoint)
	{
		if ((int)CollideAgainst != 0 && (int)CollideAgainst != (int)TransparentLayers)
		{
			Vector3 correctedPosition = state.GetCorrectedPosition();
			RaycastHit hitInfo = default(RaycastHit);
			Vector3 vector = PullCameraInFrontOfNearestObstacle(correctedPosition, lookAtPoint, (int)CollideAgainst & ~(int)TransparentLayers, ref hitInfo);
			if (hitInfo.collider != null)
			{
				extra.AddPointToDebugPath(vector, hitInfo.collider);
				if (AvoidObstacles.Strategy != 0)
				{
					Vector3 pushDir = correctedPosition - lookAtPoint;
					vector = PushCameraBack(vector, pushDir, hitInfo, lookAtPoint, new Plane(state.ReferenceUp, correctedPosition), pushDir.magnitude, AvoidObstacles.MaximumEffort, ref extra);
				}
			}
			return vector - correctedPosition;
		}
		return Vector3.zero;
	}

	private Vector3 PullCameraInFrontOfNearestObstacle(Vector3 cameraPos, Vector3 lookAtPos, int layerMask, ref RaycastHit hitInfo)
	{
		Vector3 vector = cameraPos;
		Vector3 vector2 = cameraPos - lookAtPos;
		float magnitude = vector2.magnitude;
		if (magnitude > 0.0001f)
		{
			vector2 /= magnitude;
			float num = MinimumDistanceFromTarget + AvoidObstacles.CameraRadius + 0.001f;
			if (magnitude > num)
			{
				float num2 = Mathf.Max(magnitude - num - AvoidObstacles.CameraRadius, 0.001f);
				if (AvoidObstacles.DistanceLimit > 0.0001f)
				{
					num2 = Mathf.Min(AvoidObstacles.DistanceLimit, num2);
				}
				if (RuntimeUtility.SphereCastIgnoreTag(new Ray(lookAtPos + vector2 * num, vector2), AvoidObstacles.CameraRadius, out hitInfo, num2, layerMask, in IgnoreTag))
				{
					vector = hitInfo.point + hitInfo.normal * (AvoidObstacles.CameraRadius + 0.001f);
				}
				if ((lookAtPos - vector).sqrMagnitude < num * num)
				{
					vector = lookAtPos + vector2 * num;
				}
			}
		}
		return vector;
	}

	private Vector3 PushCameraBack(Vector3 currentPos, Vector3 pushDir, RaycastHit obstacle, Vector3 lookAtPos, Plane startPlane, float targetDistance, int iterations, ref VcamExtraState extra)
	{
		Vector3 vector = currentPos;
		Vector3 outDir = Vector3.zero;
		if (obstacle.collider == null || !GetWalkingDirection(vector, pushDir, obstacle, ref outDir))
		{
			return vector;
		}
		Ray ray = new Ray(vector, outDir);
		float pushBackDistance = GetPushBackDistance(ray, startPlane, targetDistance, lookAtPos);
		if (pushBackDistance <= 0.0001f)
		{
			return vector;
		}
		float num = ClampRayToBounds(ray, pushBackDistance, obstacle.collider.bounds);
		pushBackDistance = Mathf.Min(pushBackDistance, num + 0.001f);
		if (RuntimeUtility.SphereCastIgnoreTag(ray, AvoidObstacles.CameraRadius, out var hitInfo, pushBackDistance, (int)CollideAgainst & ~(int)TransparentLayers, in IgnoreTag))
		{
			float distance = hitInfo.distance - 0.001f;
			vector = ray.GetPoint(distance);
			extra.AddPointToDebugPath(vector, hitInfo.collider);
			if (iterations > 1)
			{
				vector = PushCameraBack(vector, outDir, hitInfo, lookAtPos, startPlane, targetDistance, iterations - 1, ref extra);
			}
			return vector;
		}
		vector = ray.GetPoint(pushBackDistance);
		outDir = vector - lookAtPos;
		float magnitude = outDir.magnitude;
		if (magnitude < 0.0001f || RuntimeUtility.SphereCastIgnoreTag(new Ray(lookAtPos, outDir), AvoidObstacles.CameraRadius, out var _, magnitude - 0.001f, (int)CollideAgainst & ~(int)TransparentLayers, in IgnoreTag))
		{
			return currentPos;
		}
		ray = new Ray(vector, outDir);
		extra.AddPointToDebugPath(vector, null);
		pushBackDistance = GetPushBackDistance(ray, startPlane, targetDistance, lookAtPos);
		if (pushBackDistance > 0.0001f)
		{
			if (!RuntimeUtility.SphereCastIgnoreTag(ray, AvoidObstacles.CameraRadius, out hitInfo, pushBackDistance, (int)CollideAgainst & ~(int)TransparentLayers, in IgnoreTag))
			{
				vector = ray.GetPoint(pushBackDistance);
				extra.AddPointToDebugPath(vector, null);
			}
			else
			{
				float distance2 = hitInfo.distance - 0.001f;
				vector = ray.GetPoint(distance2);
				extra.AddPointToDebugPath(vector, hitInfo.collider);
				if (iterations > 1)
				{
					vector = PushCameraBack(vector, outDir, hitInfo, lookAtPos, startPlane, targetDistance, iterations - 1, ref extra);
				}
			}
		}
		return vector;
	}

	private bool GetWalkingDirection(Vector3 pos, Vector3 pushDir, RaycastHit obstacle, ref Vector3 outDir)
	{
		Vector3 normal = obstacle.normal;
		float num = 0.0050000004f;
		int num2 = Physics.SphereCastNonAlloc(pos, num, pushDir.normalized, m_CornerBuffer, 0f, (int)CollideAgainst & ~(int)TransparentLayers, QueryTriggerInteraction.Ignore);
		if (num2 > 1)
		{
			for (int i = 0; i < num2; i++)
			{
				if (m_CornerBuffer[i].collider == null || (IgnoreTag.Length > 0 && m_CornerBuffer[i].collider.CompareTag(IgnoreTag)))
				{
					continue;
				}
				Type type = m_CornerBuffer[i].collider.GetType();
				if (!(type == typeof(BoxCollider)) && !(type == typeof(SphereCollider)) && !(type == typeof(CapsuleCollider)))
				{
					continue;
				}
				Vector3 direction = m_CornerBuffer[i].collider.ClosestPoint(pos) - pos;
				if (direction.magnitude > 1E-05f && m_CornerBuffer[i].collider.Raycast(new Ray(pos, direction), out m_CornerBuffer[i], num))
				{
					if (!(m_CornerBuffer[i].normal - obstacle.normal).AlmostZero())
					{
						normal = m_CornerBuffer[i].normal;
					}
					break;
				}
			}
		}
		Vector3 vector = Vector3.Cross(obstacle.normal, normal);
		if (vector.AlmostZero())
		{
			vector = Vector3.ProjectOnPlane(pushDir, obstacle.normal);
		}
		else
		{
			float num3 = Vector3.Dot(vector, pushDir);
			if (Mathf.Abs(num3) < 0.0001f)
			{
				return false;
			}
			if (num3 < 0f)
			{
				vector = -vector;
			}
		}
		if (vector.AlmostZero())
		{
			return false;
		}
		outDir = vector.normalized;
		return true;
	}

	private float GetPushBackDistance(Ray ray, Plane startPlane, float targetDistance, Vector3 lookAtPos)
	{
		float num = targetDistance - (ray.origin - lookAtPos).magnitude;
		if (num < 0.0001f)
		{
			return 0f;
		}
		if (AvoidObstacles.Strategy == ObstacleAvoidance.ResolutionStrategy.PreserveCameraDistance)
		{
			return num;
		}
		if (!startPlane.Raycast(ray, out var enter))
		{
			enter = 0f;
		}
		enter = Mathf.Min(num, enter);
		if (enter < 0.0001f)
		{
			return 0f;
		}
		float num2 = Mathf.Abs(UnityVectorExtensions.Angle(startPlane.normal, ray.direction) - 90f);
		return Mathf.Lerp(0f, enter, num2 / 0.1f);
	}

	private static float ClampRayToBounds(Ray ray, float distance, Bounds bounds)
	{
		float enter;
		if (Vector3.Dot(ray.direction, Vector3.up) > 0f)
		{
			if (new Plane(Vector3.down, bounds.max).Raycast(ray, out enter) && enter > 0.0001f)
			{
				distance = Mathf.Min(distance, enter);
			}
		}
		else if (Vector3.Dot(ray.direction, Vector3.down) > 0f && new Plane(Vector3.up, bounds.min).Raycast(ray, out enter) && enter > 0.0001f)
		{
			distance = Mathf.Min(distance, enter);
		}
		if (Vector3.Dot(ray.direction, Vector3.right) > 0f)
		{
			if (new Plane(Vector3.left, bounds.max).Raycast(ray, out enter) && enter > 0.0001f)
			{
				distance = Mathf.Min(distance, enter);
			}
		}
		else if (Vector3.Dot(ray.direction, Vector3.left) > 0f && new Plane(Vector3.right, bounds.min).Raycast(ray, out enter) && enter > 0.0001f)
		{
			distance = Mathf.Min(distance, enter);
		}
		if (Vector3.Dot(ray.direction, Vector3.forward) > 0f)
		{
			if (new Plane(Vector3.back, bounds.max).Raycast(ray, out enter) && enter > 0.0001f)
			{
				distance = Mathf.Min(distance, enter);
			}
		}
		else if (Vector3.Dot(ray.direction, Vector3.back) > 0f && new Plane(Vector3.forward, bounds.min).Raycast(ray, out enter) && enter > 0.0001f)
		{
			distance = Mathf.Min(distance, enter);
		}
		return distance;
	}

	private Vector3 RespectCameraRadius(Vector3 cameraPos, Vector3 lookAtPos)
	{
		Vector3 vector = Vector3.zero;
		if (AvoidObstacles.CameraRadius < 0.0001f || (int)CollideAgainst == 0)
		{
			return vector;
		}
		Vector3 vector2 = cameraPos - lookAtPos;
		float magnitude = vector2.magnitude;
		if (magnitude > 0.0001f)
		{
			vector2 /= magnitude;
		}
		int num = Physics.OverlapSphereNonAlloc(cameraPos, AvoidObstacles.CameraRadius, s_ColliderBuffer, CollideAgainst, QueryTriggerInteraction.Ignore);
		RaycastHit hitInfo;
		if (num == 0 && (int)TransparentLayers != 0 && magnitude > MinimumDistanceFromTarget + 0.0001f)
		{
			float num2 = magnitude - MinimumDistanceFromTarget;
			if (RuntimeUtility.SphereCastIgnoreTag(new Ray(lookAtPos + vector2 * MinimumDistanceFromTarget, vector2), AvoidObstacles.CameraRadius, out hitInfo, num2, CollideAgainst, in IgnoreTag))
			{
				Collider collider = hitInfo.collider;
				if (!collider.Raycast(new Ray(cameraPos, -vector2), out hitInfo, num2))
				{
					s_ColliderBuffer[num++] = collider;
				}
			}
		}
		if ((num > 0 && magnitude == 0f) || magnitude > MinimumDistanceFromTarget)
		{
			SphereCollider scratchCollider = RuntimeUtility.GetScratchCollider();
			scratchCollider.radius = AvoidObstacles.CameraRadius;
			Vector3 vector3 = cameraPos;
			for (int i = 0; i < num; i++)
			{
				Collider collider2 = s_ColliderBuffer[i];
				if (IgnoreTag.Length > 0 && collider2.CompareTag(IgnoreTag))
				{
					continue;
				}
				if (magnitude > MinimumDistanceFromTarget)
				{
					vector2 = vector3 - lookAtPos;
					float magnitude2 = vector2.magnitude;
					if (magnitude2 > 0.0001f)
					{
						vector2 /= magnitude2;
						Ray ray = new Ray(lookAtPos, vector2);
						if (collider2.Raycast(ray, out hitInfo, magnitude2 + AvoidObstacles.CameraRadius))
						{
							vector3 = ray.GetPoint(hitInfo.distance) - vector2 * 0.001f;
						}
					}
				}
				if (Physics.ComputePenetration(scratchCollider, vector3, Quaternion.identity, collider2, collider2.transform.position, collider2.transform.rotation, out var direction, out var distance))
				{
					vector3 += direction * distance;
				}
			}
			vector = vector3 - cameraPos;
		}
		if (magnitude > 0.0001f && MinimumDistanceFromTarget > 0.0001f)
		{
			float num3 = Mathf.Max(MinimumDistanceFromTarget, AvoidObstacles.CameraRadius) + 0.001f;
			if ((cameraPos + vector - lookAtPos).magnitude < num3)
			{
				vector = lookAtPos - cameraPos + vector2 * num3;
			}
		}
		return vector;
	}

	private bool IsTargetObscured(CameraState state)
	{
		if (state.HasLookAt())
		{
			Vector3 referenceLookAt = state.ReferenceLookAt;
			Vector3 correctedPosition = state.GetCorrectedPosition();
			Vector3 vector = referenceLookAt - correctedPosition;
			float magnitude = vector.magnitude;
			if (magnitude < Mathf.Max(MinimumDistanceFromTarget, 0.0001f))
			{
				return true;
			}
			if (RuntimeUtility.SphereCastIgnoreTag(new Ray(correctedPosition, vector.normalized), AvoidObstacles.CameraRadius, out var _, magnitude - MinimumDistanceFromTarget, (int)CollideAgainst & ~(int)TransparentLayers, in IgnoreTag))
			{
				return true;
			}
		}
		return false;
	}
}
