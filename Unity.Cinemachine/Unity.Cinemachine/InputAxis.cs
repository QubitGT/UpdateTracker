using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct InputAxis
{
	[Serializable]
	public struct RecenteringSettings
	{
		[Tooltip("If set, will enable automatic re-centering of the axis")]
		public bool Enabled;

		[Tooltip("If no user input has been detected on the axis for this many seconds, re-centering will begin.")]
		public float Wait;

		[Tooltip("How long it takes to reach center once re-centering has started.")]
		public float Time;

		public static RecenteringSettings Default
		{
			get
			{
				RecenteringSettings result = default(RecenteringSettings);
				result.Wait = 1f;
				result.Time = 2f;
				return result;
			}
		}

		public void Validate()
		{
			Wait = Mathf.Max(0f, Wait);
			Time = Mathf.Max(0f, Time);
		}
	}

	[Flags]
	public enum RestrictionFlags
	{
		None = 0,
		RangeIsDriven = 1,
		NoRecentering = 2,
		Momentary = 4
	}

	private struct RecenteringState
	{
		public const float k_Epsilon = 0.0001f;

		public float m_RecenteringVelocity;

		public bool m_ForceRecenter;

		public float m_LastValueChangeTime;

		public float m_LastValue;

		public static float CurrentTime => CinemachineCore.CurrentUnscaledTime;
	}

	[Tooltip("The current value of the axis.  You can drive this directly from a script.")]
	[NoSaveDuringPlay]
	public float Value;

	[Delayed]
	[Tooltip("The centered, or at-rest value of this axis.")]
	public float Center;

	[Tooltip("The valid range for the axis value.  Value will be clamped to this range.")]
	[Vector2AsRange]
	public Vector2 Range;

	[Tooltip("If set, then the axis will wrap around at the min/max values, forming a loop")]
	public bool Wrap;

	[FoldoutWithEnabledButton("Enabled")]
	public RecenteringSettings Recentering;

	[HideInInspector]
	public RestrictionFlags Restrictions;

	private RecenteringState m_RecenteringState;

	public static InputAxis DefaultMomentary
	{
		get
		{
			InputAxis result = default(InputAxis);
			result.Range = new Vector2(-1f, 1f);
			result.Restrictions = RestrictionFlags.NoRecentering | RestrictionFlags.Momentary;
			return result;
		}
	}

	public float ClampValue(float v)
	{
		float num = Range.y - Range.x;
		if (!Wrap || num < 0.0001f)
		{
			return Mathf.Clamp(v, Range.x, Range.y);
		}
		float num2 = (v - Range.x) % num;
		return num2 + ((num2 < 0f) ? num : 0f) + Range.x;
	}

	public float GetNormalizedValue()
	{
		float num = ClampValue(Value);
		float num2 = Range.y - Range.x;
		return (num - Range.x) / ((num2 > 0.0001f) ? num2 : 1f);
	}

	public float GetClampedValue()
	{
		return ClampValue(Value);
	}

	public void Validate()
	{
		Range.y = Mathf.Max(Range.x, Range.y);
		Center = ClampValue(Center);
		Value = ClampValue(Value);
		Recentering.Validate();
	}

	public void Reset()
	{
		CancelRecentering();
		if (Recentering.Enabled && (Restrictions & RestrictionFlags.NoRecentering) == 0)
		{
			Value = ClampValue(Center);
		}
	}

	public bool TrackValueChange()
	{
		float num = ClampValue(Value);
		if (num != m_RecenteringState.m_LastValue)
		{
			m_RecenteringState.m_LastValueChangeTime = RecenteringState.CurrentTime;
			m_RecenteringState.m_LastValue = num;
			return true;
		}
		return false;
	}

	internal void SetValueAndLastValue(float value)
	{
		Value = (m_RecenteringState.m_LastValue = value);
	}

	public void UpdateRecentering(float deltaTime, bool forceCancel)
	{
		UpdateRecentering(deltaTime, forceCancel, Center);
	}

	public void UpdateRecentering(float deltaTime, bool forceCancel, float center)
	{
		if ((Restrictions & (RestrictionFlags.NoRecentering | RestrictionFlags.Momentary)) != 0)
		{
			return;
		}
		if (forceCancel)
		{
			CancelRecentering();
		}
		else if ((m_RecenteringState.m_ForceRecenter || Recentering.Enabled) && deltaTime < 0f)
		{
			Value = ClampValue(center);
			CancelRecentering();
		}
		else
		{
			if (!m_RecenteringState.m_ForceRecenter && (!Recentering.Enabled || !(RecenteringState.CurrentTime - m_RecenteringState.m_LastValueChangeTime >= Recentering.Wait)))
			{
				return;
			}
			float num = ClampValue(Value);
			float num2 = Mathf.Abs(center - num);
			if (num2 < 0.0001f || Recentering.Time < 0.0001f)
			{
				num = center;
				m_RecenteringState.m_RecenteringVelocity = 0f;
			}
			else
			{
				float num3 = Range.y - Range.x;
				if (Wrap && num2 > num3 * 0.5f)
				{
					num += Mathf.Sign(center - num) * num3;
				}
				num = Mathf.SmoothDamp(num, center, ref m_RecenteringState.m_RecenteringVelocity, Recentering.Time * 0.5f, 9999f, deltaTime);
			}
			Value = (m_RecenteringState.m_LastValue = ClampValue(num));
			if (Mathf.Abs(Value - center) < 0.0001f)
			{
				m_RecenteringState.m_ForceRecenter = false;
			}
		}
	}

	public void TriggerRecentering()
	{
		m_RecenteringState.m_ForceRecenter = true;
	}

	public void CancelRecentering()
	{
		m_RecenteringState.m_LastValueChangeTime = RecenteringState.CurrentTime;
		m_RecenteringState.m_LastValue = ClampValue(Value);
		m_RecenteringState.m_RecenteringVelocity = 0f;
		m_RecenteringState.m_ForceRecenter = false;
	}
}
