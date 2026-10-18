using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[Obsolete("CinemachineConfiner has been deprecated. Use CinemachineConfiner2D or CinemachineConfiner3D instead")]
[AddComponentMenu("")]
[SaveDuringPlay]
[ExecuteAlways]
[DisallowMultipleComponent]
public class CinemachineConfiner : CinemachineExtension
{
	public enum Mode
	{
		Confine2D,
		Confine3D
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public Vector3 PreviousDisplacement;

		public float ConfinerDisplacement;
	}

	[Tooltip("The confiner can operate using a 2D bounding shape or a 3D bounding volume")]
	public Mode m_ConfineMode;

	[Tooltip("The volume within which the camera is to be contained")]
	public Collider m_BoundingVolume;

	[Tooltip("The 2D shape within which the camera is to be contained")]
	public Collider2D m_BoundingShape2D;

	private Collider2D m_BoundingShape2DCache;

	[Tooltip("If camera is orthographic, screen edges will be confined to the volume.  If not checked, then only the camera center will be confined")]
	public bool m_ConfineScreenEdges = true;

	[Tooltip("How gradually to return the camera to the bounding volume if it goes beyond the borders.  Higher numbers are more gradual.")]
	[Range(0f, 10f)]
	public float m_Damping;

	private List<List<Vector2>> m_PathCache;

	private int m_PathTotalPointCount;

	public bool IsValid
	{
		get
		{
			if (m_ConfineMode != Mode.Confine3D || !(m_BoundingVolume != null) || !m_BoundingVolume.enabled || !m_BoundingVolume.gameObject.activeInHierarchy)
			{
				if (m_ConfineMode == Mode.Confine2D && m_BoundingShape2D != null && m_BoundingShape2D.enabled)
				{
					return m_BoundingShape2D.gameObject.activeInHierarchy;
				}
				return false;
			}
			return true;
		}
	}

	public bool CameraWasDisplaced(CinemachineVirtualCameraBase vcam)
	{
		return GetCameraDisplacementDistance(vcam) > 0f;
	}

	public float GetCameraDisplacementDistance(CinemachineVirtualCameraBase vcam)
	{
		return GetExtraState<VcamExtraState>(vcam).ConfinerDisplacement;
	}

	private void Reset()
	{
		m_ConfineMode = Mode.Confine3D;
		m_BoundingVolume = null;
		m_BoundingShape2D = null;
		m_ConfineScreenEdges = true;
		m_Damping = 0f;
	}

	private void OnValidate()
	{
		m_Damping = Mathf.Max(0f, m_Damping);
	}

	protected override void ConnectToVcam(bool connect)
	{
		base.ConnectToVcam(connect);
	}

	public override float GetMaxDampTime()
	{
		return m_Damping;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (IsValid && stage == CinemachineCore.Stage.Body)
		{
			VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
			Vector3 vector = ((!m_ConfineScreenEdges || !state.Lens.Orthographic) ? ConfinePoint(state.GetCorrectedPosition()) : ConfineOrthoCameraToScreenEdges(ref state));
			if (m_Damping > 0f && deltaTime >= 0f && vcam.PreviousStateIsValid)
			{
				Vector3 initial = vector - extraState.PreviousDisplacement;
				initial = Damper.Damp(initial, m_Damping, deltaTime);
				vector = extraState.PreviousDisplacement + initial;
			}
			extraState.PreviousDisplacement = vector;
			state.PositionCorrection += vector;
			extraState.ConfinerDisplacement = vector.magnitude;
		}
	}

	[Obsolete("Please use InvalidateCache() instead")]
	public void InvalidatePathCache()
	{
		InvalidatePathCache();
	}

	public void InvalidateCache()
	{
		m_PathCache = null;
		m_BoundingShape2DCache = null;
	}

	private bool ValidatePathCache()
	{
		if (m_BoundingShape2DCache != m_BoundingShape2D)
		{
			InvalidateCache();
			m_BoundingShape2DCache = m_BoundingShape2D;
		}
		Type type = ((m_BoundingShape2D == null) ? null : m_BoundingShape2D.GetType());
		if (type == typeof(PolygonCollider2D))
		{
			PolygonCollider2D polygonCollider2D = m_BoundingShape2D as PolygonCollider2D;
			if (m_PathCache == null || m_PathCache.Count != polygonCollider2D.pathCount || m_PathTotalPointCount != polygonCollider2D.GetTotalPointCount())
			{
				m_PathCache = new List<List<Vector2>>();
				for (int i = 0; i < polygonCollider2D.pathCount; i++)
				{
					Vector2[] path = polygonCollider2D.GetPath(i);
					List<Vector2> list = new List<Vector2>();
					for (int j = 0; j < path.Length; j++)
					{
						list.Add(path[j]);
					}
					m_PathCache.Add(list);
				}
				m_PathTotalPointCount = polygonCollider2D.GetTotalPointCount();
			}
			return true;
		}
		if (type == typeof(CompositeCollider2D))
		{
			CompositeCollider2D compositeCollider2D = m_BoundingShape2D as CompositeCollider2D;
			if (m_PathCache == null || m_PathCache.Count != compositeCollider2D.pathCount || m_PathTotalPointCount != compositeCollider2D.pointCount)
			{
				m_PathCache = new List<List<Vector2>>();
				Vector2[] array = new Vector2[compositeCollider2D.pointCount];
				Vector3 lossyScale = m_BoundingShape2D.transform.lossyScale;
				Vector2 vector = new Vector2(1f / lossyScale.x, 1f / lossyScale.y);
				for (int k = 0; k < compositeCollider2D.pathCount; k++)
				{
					int path2 = compositeCollider2D.GetPath(k, array);
					List<Vector2> list2 = new List<Vector2>();
					for (int l = 0; l < path2; l++)
					{
						list2.Add(array[l] * vector);
					}
					m_PathCache.Add(list2);
				}
				m_PathTotalPointCount = compositeCollider2D.pointCount;
			}
			return true;
		}
		InvalidateCache();
		return false;
	}

	private Vector3 ConfinePoint(Vector3 camPos)
	{
		if (m_ConfineMode == Mode.Confine3D)
		{
			return m_BoundingVolume.ClosestPoint(camPos) - camPos;
		}
		Vector2 vector = camPos;
		Vector2 vector2 = vector;
		if (m_BoundingShape2D.OverlapPoint(camPos))
		{
			return Vector3.zero;
		}
		if (!ValidatePathCache())
		{
			return Vector3.zero;
		}
		float num = float.MaxValue;
		for (int i = 0; i < m_PathCache.Count; i++)
		{
			int count = m_PathCache[i].Count;
			if (count <= 0)
			{
				continue;
			}
			Vector3 vector3 = m_BoundingShape2D.transform.TransformPoint(m_PathCache[i][count - 1] + m_BoundingShape2D.offset);
			for (int j = 0; j < count; j++)
			{
				Vector2 vector4 = m_BoundingShape2D.transform.TransformPoint(m_PathCache[i][j] + m_BoundingShape2D.offset);
				Vector2 vector5 = Vector2.Lerp(vector3, vector4, vector.ClosestPointOnSegment((Vector2)vector3, vector4));
				float num2 = Vector2.SqrMagnitude(vector - vector5);
				if (num2 < num)
				{
					num = num2;
					vector2 = vector5;
				}
				vector3 = vector4;
			}
		}
		return vector2 - vector;
	}

	private Vector3 ConfineOrthoCameraToScreenEdges(ref CameraState state)
	{
		Quaternion correctedOrientation = state.GetCorrectedOrientation();
		float orthographicSize = state.Lens.OrthographicSize;
		float num = orthographicSize * state.Lens.Aspect;
		Vector3 vector = correctedOrientation * Vector3.right * num;
		Vector3 vector2 = correctedOrientation * Vector3.up * orthographicSize;
		Vector3 zero = Vector3.zero;
		Vector3 correctedPosition = state.GetCorrectedPosition();
		Vector3 vector3 = Vector3.zero;
		for (int i = 0; i < 12; i++)
		{
			Vector3 vector4 = ConfinePoint(correctedPosition - vector2 - vector);
			if (vector4.AlmostZero())
			{
				vector4 = ConfinePoint(correctedPosition + vector2 + vector);
			}
			if (vector4.AlmostZero())
			{
				vector4 = ConfinePoint(correctedPosition - vector2 + vector);
			}
			if (vector4.AlmostZero())
			{
				vector4 = ConfinePoint(correctedPosition + vector2 - vector);
			}
			if (vector4.AlmostZero())
			{
				break;
			}
			if ((vector4 + vector3).AlmostZero())
			{
				zero += vector4 * 0.5f;
				break;
			}
			zero += vector4;
			correctedPosition += vector4;
			vector3 = vector4;
		}
		return zero;
	}

	internal Type UpgradeToCm3_GetTargetType()
	{
		if (m_ConfineMode != Mode.Confine3D)
		{
			return typeof(CinemachineConfiner2D);
		}
		return typeof(CinemachineConfiner3D);
	}

	internal void UpgradeToCm3(CinemachineConfiner3D c)
	{
		c.BoundingVolume = m_BoundingVolume;
	}

	internal void UpgradeToCm3(CinemachineConfiner2D c)
	{
		c.BoundingShape2D = m_BoundingShape2D;
		c.Damping = m_Damping;
	}
}
