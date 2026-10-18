using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Confiner 2D")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineConfiner2D.html")]
public class CinemachineConfiner2D : CinemachineExtension
{
	[Serializable]
	public struct OversizeWindowSettings
	{
		[Tooltip("Enable optimizing of computation and memory costs in the event that the window size is expected to be larger than will fit inside the confining shape.\nEnable only if needed, because it's costly")]
		public bool Enabled;

		[Tooltip("To optimize computation and memory costs, set this to the largest view size that the camera is expected to have.  The confiner will not compute a polygon cache for frustum sizes larger than this.  This refers to the size in world units of the frustum at the confiner plane (for orthographic cameras, this is just the orthographic size).  If set to 0, then this parameter is ignored and a polygon cache will be calculated for all potential window sizes.")]
		public float MaxWindowSize;

		[Tooltip("For large window sizes, the confiner will potentially generate polygons with zero area.  The padding may be used to add a small amount of area to these polygons, to prevent them from being a series of disconnected dots.")]
		[Range(0f, 100f)]
		public float Padding;
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public ConfinerOven.BakedSolution BakedSolution;

		public Vector3 PreviousDisplacement;

		public Vector3 DampedDisplacement;

		public Vector3 PreviousCameraPosition;

		public float FrustumHeight;
	}

	private struct ShapeCache
	{
		public ConfinerOven ConfinerOven;

		public List<List<Vector2>> OriginalPath;

		public Matrix4x4 DeltaWorldToBaked;

		public Matrix4x4 DeltaBakedToWorld;

		public float AspectRatio;

		private OversizeWindowSettings m_OversizeWindowSettings;

		internal float MaxComputationTimePerFrameInSeconds;

		private Matrix4x4 m_BakedToWorld;

		private Collider2D m_BoundingShape2D;

		public void Invalidate()
		{
			m_OversizeWindowSettings = default(OversizeWindowSettings);
			DeltaBakedToWorld = (DeltaWorldToBaked = Matrix4x4.identity);
			m_BoundingShape2D = null;
			OriginalPath = null;
			ConfinerOven = null;
		}

		public bool ValidateCache(Collider2D boundingShape2D, OversizeWindowSettings oversize, float aspectRatio, out bool confinerStateChanged)
		{
			confinerStateChanged = false;
			if (IsValid(in boundingShape2D, in oversize, aspectRatio))
			{
				if (ConfinerOven.State == ConfinerOven.BakingState.BAKING)
				{
					ConfinerOven.BakeConfiner(MaxComputationTimePerFrameInSeconds);
					confinerStateChanged = ConfinerOven.State != ConfinerOven.BakingState.BAKING;
				}
				CalculateDeltaTransformationMatrix();
				if (((Vector2)DeltaWorldToBaked.lossyScale).IsUniform())
				{
					return true;
				}
			}
			Invalidate();
			if (boundingShape2D == null)
			{
				return false;
			}
			confinerStateChanged = true;
			if (!(boundingShape2D is PolygonCollider2D polygonCollider2D))
			{
				if (!(boundingShape2D is BoxCollider2D boxCollider2D))
				{
					if (!(boundingShape2D is CompositeCollider2D compositeCollider2D))
					{
						return false;
					}
					OriginalPath = new List<List<Vector2>>();
					m_BakedToWorld = boundingShape2D.transform.localToWorldMatrix;
					Vector2[] array = new Vector2[compositeCollider2D.pointCount];
					for (int i = 0; i < compositeCollider2D.pathCount; i++)
					{
						int path = compositeCollider2D.GetPath(i, array);
						List<Vector2> list = new List<Vector2>();
						for (int j = 0; j < path; j++)
						{
							list.Add(m_BakedToWorld.MultiplyPoint3x4(array[j]));
						}
						OriginalPath.Add(list);
					}
				}
				else
				{
					m_BakedToWorld = boundingShape2D.transform.localToWorldMatrix;
					Vector2 size = boxCollider2D.size;
					float num = size.y / 2f;
					float num2 = size.x / 2f;
					Vector3 vector = m_BakedToWorld.MultiplyPoint3x4(new Vector3(0f - num2, num));
					Vector3 vector2 = m_BakedToWorld.MultiplyPoint3x4(new Vector3(num2, num));
					Vector3 vector3 = m_BakedToWorld.MultiplyPoint3x4(new Vector3(num2, 0f - num));
					Vector3 vector4 = m_BakedToWorld.MultiplyPoint3x4(new Vector3(0f - num2, 0f - num));
					OriginalPath = new List<List<Vector2>>
					{
						new List<Vector2> { vector, vector2, vector3, vector4 }
					};
				}
			}
			else
			{
				OriginalPath = new List<List<Vector2>>();
				m_BakedToWorld = boundingShape2D.transform.localToWorldMatrix;
				for (int k = 0; k < polygonCollider2D.pathCount; k++)
				{
					Vector2[] path2 = polygonCollider2D.GetPath(k);
					List<Vector2> list2 = new List<Vector2>();
					for (int l = 0; l < path2.Length; l++)
					{
						list2.Add(m_BakedToWorld.MultiplyPoint3x4(path2[l]));
					}
					OriginalPath.Add(list2);
				}
			}
			if (!HasAnyPoints(OriginalPath))
			{
				return false;
			}
			ConfinerOven = new ConfinerOven(in OriginalPath, in aspectRatio, oversize.Enabled ? oversize.MaxWindowSize : (-1f), oversize.Padding);
			m_BoundingShape2D = boundingShape2D;
			m_OversizeWindowSettings = oversize;
			AspectRatio = aspectRatio;
			CalculateDeltaTransformationMatrix();
			return true;
			static bool HasAnyPoints(List<List<Vector2>> originalPath)
			{
				for (int m = 0; m < originalPath.Count; m++)
				{
					if (originalPath[m].Count != 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		private bool IsValid(in Collider2D boundingShape2D, in OversizeWindowSettings oversize, float aspectRatio)
		{
			if (boundingShape2D != null && m_BoundingShape2D != null && m_BoundingShape2D == boundingShape2D && OriginalPath != null && ConfinerOven != null && Math.Abs(AspectRatio - aspectRatio) < 0.0001f && m_OversizeWindowSettings.Enabled == oversize.Enabled && m_OversizeWindowSettings.Padding == oversize.Padding)
			{
				return Mathf.Abs(m_OversizeWindowSettings.MaxWindowSize - oversize.MaxWindowSize) < 0.0001f;
			}
			return false;
		}

		private void CalculateDeltaTransformationMatrix()
		{
			Matrix4x4 matrix4x = Matrix4x4.Translate(-m_BoundingShape2D.offset) * m_BoundingShape2D.transform.worldToLocalMatrix;
			DeltaWorldToBaked = m_BakedToWorld * matrix4x;
			DeltaBakedToWorld = DeltaWorldToBaked.inverse;
		}
	}

	[Tooltip("The 2D shape within which the camera is to be contained.  Can be polygon-, box-, or composite collider 2D.\n\nRemark: When assigning a GameObject here in the editor, this will be set to the first Collider2D found on the assigned GameObject!")]
	[FormerlySerializedAs("m_BoundingShape2D")]
	public Collider2D BoundingShape2D;

	[Tooltip("Damping applied around corners to avoid jumps.  Higher numbers are more gradual.")]
	[Range(0f, 5f)]
	[FormerlySerializedAs("m_Damping")]
	public float Damping;

	[Tooltip("Size of the slow-down zone at the edge of the bounding shape.")]
	public float SlowingDistance;

	[FoldoutWithEnabledButton("Enabled")]
	public OversizeWindowSettings OversizeWindow;

	private List<VcamExtraState> m_ExtraStateCache;

	private ShapeCache m_ShapeCache;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_MaxWindowSize")]
	private float m_LegacyMaxWindowSize = -2f;

	private const float k_CornerAngleThreshold = 10f;

	public bool BoundingShapeIsBaked
	{
		get
		{
			ConfinerOven confinerOven = m_ShapeCache.ConfinerOven;
			if (confinerOven == null)
			{
				return false;
			}
			return confinerOven.State == ConfinerOven.BakingState.BAKED;
		}
	}

	private void OnValidate()
	{
		Damping = Mathf.Max(0f, Damping);
		SlowingDistance = Mathf.Max(0f, SlowingDistance);
		m_ShapeCache.MaxComputationTimePerFrameInSeconds = 1f / 120f;
		OversizeWindow.MaxWindowSize = Mathf.Max(0f, OversizeWindow.MaxWindowSize);
		if (m_LegacyMaxWindowSize != -2f)
		{
			OversizeWindow = new OversizeWindowSettings
			{
				Enabled = (m_LegacyMaxWindowSize >= 0f),
				MaxWindowSize = Mathf.Max(0f, m_LegacyMaxWindowSize)
			};
			m_LegacyMaxWindowSize = -2f;
		}
	}

	private void Reset()
	{
		Damping = 0.5f;
		SlowingDistance = 5f;
		OversizeWindow = default(OversizeWindowSettings);
	}

	public override float GetMaxDampTime()
	{
		return Mathf.Max(Damping, SlowingDistance * 0.2f);
	}

	public override void OnTargetObjectWarped(CinemachineVirtualCameraBase vcam, Transform target, Vector3 positionDelta)
	{
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		if (extraState.Vcam.Follow == target)
		{
			extraState.PreviousCameraPosition += positionDelta;
		}
	}

	public void InvalidateLensCache()
	{
		if (m_ExtraStateCache == null)
		{
			m_ExtraStateCache = new List<VcamExtraState>();
		}
		GetAllExtraStates(m_ExtraStateCache);
		for (int i = 0; i < m_ExtraStateCache.Count; i++)
		{
			VcamExtraState vcamExtraState = m_ExtraStateCache[i];
			if (vcamExtraState.Vcam != null)
			{
				vcamExtraState.BakedSolution = null;
				vcamExtraState.FrustumHeight = 0f;
			}
		}
	}

	public void InvalidateBoundingShapeCache()
	{
		m_ShapeCache.Invalidate();
		InvalidateLensCache();
	}

	[Obsolete("Call InvalidateBoundingShapeCache() instead.", false)]
	public void InvalidateCache()
	{
		InvalidateBoundingShapeCache();
	}

	public bool BakeBoundingShape(CinemachineVirtualCameraBase vcam, float maxTimeInSeconds)
	{
		if (!m_ShapeCache.ValidateCache(BoundingShape2D, OversizeWindow, vcam.State.Lens.Aspect, out var _))
		{
			return false;
		}
		if (m_ShapeCache.ConfinerOven == null)
		{
			return false;
		}
		if (m_ShapeCache.ConfinerOven.State == ConfinerOven.BakingState.BAKING)
		{
			m_ShapeCache.ConfinerOven.BakeConfiner(maxTimeInSeconds);
		}
		return m_ShapeCache.ConfinerOven.State == ConfinerOven.BakingState.BAKED;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage != 0)
		{
			return;
		}
		float aspect = state.Lens.Aspect;
		if (!m_ShapeCache.ValidateCache(BoundingShape2D, OversizeWindow, aspect, out var confinerStateChanged))
		{
			return;
		}
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		Vector3 correctedPosition = state.GetCorrectedPosition();
		if (confinerStateChanged || extraState.BakedSolution == null || !extraState.BakedSolution.IsValid())
		{
			Matrix4x4 deltaWorldToBaked = m_ShapeCache.DeltaWorldToBaked;
			m_ShapeCache.AspectRatio = aspect;
			ref LensSettings lens = ref state.Lens;
			Vector3 vector = deltaWorldToBaked.MultiplyPoint3x4(correctedPosition);
			extraState.FrustumHeight = CalculateHalfFrustumHeight(in lens, in vector.z) * deltaWorldToBaked.lossyScale.x;
			extraState.BakedSolution = m_ShapeCache.ConfinerOven.GetBakedSolution(extraState.FrustumHeight);
		}
		Vector3 fwd = state.GetCorrectedOrientation() * Vector3.forward;
		Vector3 vector2 = ConfinePoint(correctedPosition, extraState, fwd);
		if (SlowingDistance > 0.0001f && deltaTime >= 0f && vcam.PreviousStateIsValid)
		{
			Vector3 previousCameraPosition = extraState.PreviousCameraPosition;
			Vector3 vector3 = vector2 - previousCameraPosition;
			float magnitude = vector3.magnitude;
			if (magnitude > 0.0001f)
			{
				float num = GetDistanceFromEdge(previousCameraPosition, vector3 / magnitude, SlowingDistance, extraState, fwd) / SlowingDistance;
				vector2 = Vector3.Lerp(previousCameraPosition, vector2, num * num * num + 0.05f);
			}
		}
		Vector3 previousDisplacement = extraState.PreviousDisplacement;
		Vector3 vector4 = (extraState.PreviousDisplacement = vector2 - correctedPosition);
		if (!vcam.PreviousStateIsValid || deltaTime < 0f || Damping <= 0f)
		{
			extraState.DampedDisplacement = Vector3.zero;
		}
		else
		{
			if (previousDisplacement.sqrMagnitude > 0.01f && Vector2.Angle(previousDisplacement, vector4) > 10f)
			{
				extraState.DampedDisplacement += vector4 - previousDisplacement;
			}
			extraState.DampedDisplacement -= Damper.Damp(extraState.DampedDisplacement, Damping, deltaTime);
			vector4 -= extraState.DampedDisplacement;
		}
		state.PositionCorrection += vector4;
		extraState.PreviousCameraPosition = state.GetCorrectedPosition();
	}

	private Vector3 ConfinePoint(Vector3 pos, VcamExtraState extra, Vector3 fwd)
	{
		Vector3 vector = m_ShapeCache.DeltaWorldToBaked.MultiplyPoint3x4(pos);
		ref Matrix4x4 deltaBakedToWorld = ref m_ShapeCache.DeltaBakedToWorld;
		ConfinerOven.BakedSolution bakedSolution = extra.BakedSolution;
		Vector2 pointToConfine = vector;
		Vector3 vector2 = deltaBakedToWorld.MultiplyPoint3x4(bakedSolution.ConfinePoint(in pointToConfine));
		return vector2 - fwd * Vector3.Dot(fwd, vector2 - pos);
	}

	private float GetDistanceFromEdge(Vector3 p, Vector3 dirUnit, float max, VcamExtraState extra, Vector3 fwd)
	{
		p += dirUnit * max;
		return Mathf.Max(0f, max - (ConfinePoint(p, extra, fwd) - p).magnitude);
	}

	public static float CalculateHalfFrustumHeight(in LensSettings lens, in float cameraPosLocalZ)
	{
		float f = ((!lens.Orthographic) ? (cameraPosLocalZ * Mathf.Tan(lens.FieldOfView * 0.5f * (MathF.PI / 180f))) : lens.OrthographicSize);
		return Mathf.Abs(f);
	}
}
