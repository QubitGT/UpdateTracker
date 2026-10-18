using System;
using UnityEngine;

namespace Unity.Cinemachine;

public static class CameraStateExtensions
{
	public static bool HasLookAt(this CameraState s)
	{
		return s.ReferenceLookAt == s.ReferenceLookAt;
	}

	public static Vector3 GetCorrectedPosition(this CameraState s)
	{
		return s.RawPosition + s.PositionCorrection;
	}

	public static Quaternion GetCorrectedOrientation(this CameraState s)
	{
		return s.RawOrientation * s.OrientationCorrection;
	}

	public static Vector3 GetFinalPosition(this CameraState s)
	{
		return s.RawPosition + s.PositionCorrection;
	}

	public static Quaternion GetFinalOrientation(this CameraState s)
	{
		if (Mathf.Abs(s.Lens.Dutch) > 0.0001f)
		{
			return s.GetCorrectedOrientation() * Quaternion.AngleAxis(s.Lens.Dutch, Vector3.forward);
		}
		return s.GetCorrectedOrientation();
	}

	public static int GetNumCustomBlendables(this CameraState s)
	{
		return s.CustomBlendables.NumItems;
	}

	public static CameraState.CustomBlendableItems.Item GetCustomBlendable(this CameraState s, int index)
	{
		switch (index)
		{
		case 0:
			return s.CustomBlendables.m_Item0;
		case 1:
			return s.CustomBlendables.m_Item1;
		case 2:
			return s.CustomBlendables.m_Item2;
		case 3:
			return s.CustomBlendables.m_Item3;
		default:
			index -= 4;
			if (s.CustomBlendables.m_Overflow != null && index < s.CustomBlendables.m_Overflow.Count)
			{
				return s.CustomBlendables.m_Overflow[index];
			}
			return default(CameraState.CustomBlendableItems.Item);
		}
	}

	public static int FindCustomBlendable(this CameraState s, UnityEngine.Object custom)
	{
		if (s.CustomBlendables.m_Item0.Custom == custom)
		{
			return 0;
		}
		if (s.CustomBlendables.m_Item1.Custom == custom)
		{
			return 1;
		}
		if (s.CustomBlendables.m_Item2.Custom == custom)
		{
			return 2;
		}
		if (s.CustomBlendables.m_Item3.Custom == custom)
		{
			return 3;
		}
		if (s.CustomBlendables.m_Overflow != null)
		{
			for (int i = 0; i < s.CustomBlendables.m_Overflow.Count; i++)
			{
				if (s.CustomBlendables.m_Overflow[i].Custom == custom)
				{
					return i + 4;
				}
			}
		}
		return -1;
	}

	public static bool IsTargetOffscreen(this CameraState state)
	{
		if (state.HasLookAt())
		{
			Vector3 vector = state.ReferenceLookAt - state.GetCorrectedPosition();
			vector = Quaternion.Inverse(state.GetCorrectedOrientation()) * vector;
			if (state.Lens.Orthographic)
			{
				if (Mathf.Abs(vector.y) > state.Lens.OrthographicSize)
				{
					return true;
				}
				if (Mathf.Abs(vector.x) > state.Lens.OrthographicSize * state.Lens.Aspect)
				{
					return true;
				}
			}
			else
			{
				float num = state.Lens.FieldOfView / 2f;
				if (UnityVectorExtensions.Angle(vector.ProjectOntoPlane(Vector3.right), Vector3.forward) > num)
				{
					return true;
				}
				num = 57.29578f * Mathf.Atan(Mathf.Tan(num * (MathF.PI / 180f)) * state.Lens.Aspect);
				if (UnityVectorExtensions.Angle(vector.ProjectOntoPlane(Vector3.up), Vector3.forward) > num)
				{
					return true;
				}
			}
		}
		return false;
	}
}
