using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct DefaultInputAxisDriver
{
	private float m_CurrentSpeed;

	[Tooltip("The amount of time in seconds it takes to accelerate to MaxSpeed with the supplied Axis at its maximum value")]
	public float AccelTime;

	[Tooltip("The amount of time in seconds it takes to decelerate the axis to zero if the supplied axis is in a neutral position")]
	public float DecelTime;

	public static DefaultInputAxisDriver Default
	{
		get
		{
			DefaultInputAxisDriver result = default(DefaultInputAxisDriver);
			result.AccelTime = 0.2f;
			result.DecelTime = 0.2f;
			return result;
		}
	}

	public void Validate()
	{
		AccelTime = Mathf.Max(0f, AccelTime);
		DecelTime = Mathf.Max(0f, DecelTime);
	}

	public void ProcessInput(ref InputAxis axis, float inputValue, float deltaTime)
	{
		float dampTime = ((Mathf.Abs(inputValue) < Mathf.Abs(m_CurrentSpeed)) ? DecelTime : AccelTime);
		if ((axis.Restrictions & InputAxis.RestrictionFlags.Momentary) == 0)
		{
			if (deltaTime < 0f)
			{
				m_CurrentSpeed = 0f;
			}
			else
			{
				m_CurrentSpeed += Damper.Damp(inputValue - m_CurrentSpeed, dampTime, deltaTime);
				if (!axis.Wrap && DecelTime > 0.0001f && Mathf.Abs(m_CurrentSpeed) > 0.0001f)
				{
					float num = axis.ClampValue(axis.Value);
					float num2 = ((m_CurrentSpeed > 0f) ? (axis.Range.y - num) : (num - axis.Range.x));
					float num3 = 0.1f + 4f * num2 / DecelTime;
					if (Mathf.Abs(m_CurrentSpeed) > Mathf.Abs(num3))
					{
						m_CurrentSpeed = num3 * Mathf.Sign(m_CurrentSpeed);
					}
				}
			}
			axis.Value = axis.ClampValue(axis.Value + m_CurrentSpeed * deltaTime);
		}
		else if (deltaTime < 0f)
		{
			axis.Value = axis.Center;
		}
		else
		{
			float num4 = axis.ClampValue(inputValue + axis.Center);
			axis.Value += Damper.Damp(num4 - axis.Value, dampTime, deltaTime);
		}
	}

	public void Reset(ref InputAxis axis)
	{
		m_CurrentSpeed = 0f;
		axis.Reset();
	}
}
