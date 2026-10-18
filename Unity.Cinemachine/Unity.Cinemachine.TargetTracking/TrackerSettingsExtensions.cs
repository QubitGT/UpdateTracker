using UnityEngine;

namespace Unity.Cinemachine.TargetTracking;

public static class TrackerSettingsExtensions
{
	public static float GetMaxDampTime(this TrackerSettings s)
	{
		Vector3 effectivePositionDamping = s.GetEffectivePositionDamping();
		Vector3 vector = ((s.AngularDampingMode == AngularDampingMode.Euler) ? s.GetEffectiveRotationDamping() : new Vector3(s.QuaternionDamping, 0f, 0f));
		float a = Mathf.Max(effectivePositionDamping.x, Mathf.Max(effectivePositionDamping.y, effectivePositionDamping.z));
		float b = Mathf.Max(vector.x, Mathf.Max(vector.y, vector.z));
		return Mathf.Max(a, b);
	}

	internal static Vector3 GetEffectivePositionDamping(this TrackerSettings s)
	{
		if (s.BindingMode != BindingMode.LazyFollow)
		{
			return s.PositionDamping;
		}
		return new Vector3(0f, s.PositionDamping.y, s.PositionDamping.z);
	}

	internal static Vector3 GetEffectiveRotationDamping(this TrackerSettings s)
	{
		switch (s.BindingMode)
		{
		case BindingMode.LockToTargetNoRoll:
			return new Vector3(s.RotationDamping.x, s.RotationDamping.y, 0f);
		case BindingMode.LockToTargetWithWorldUp:
			return new Vector3(0f, s.RotationDamping.y, 0f);
		case BindingMode.WorldSpace:
		case BindingMode.LazyFollow:
			return Vector3.zero;
		default:
			return s.RotationDamping;
		}
	}
}
