using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct PrioritySettings
{
	[Tooltip("Enable this to expose the Priority field")]
	public bool Enabled;

	[Tooltip("Priority to use.  0 is default.  Camera with highest priority is prioritized.")]
	[SerializeField]
	private int m_Value;

	public int Value
	{
		readonly get
		{
			if (!Enabled)
			{
				return 0;
			}
			return m_Value;
		}
		set
		{
			m_Value = value;
			Enabled = true;
		}
	}

	public static implicit operator int(PrioritySettings prioritySettings)
	{
		return prioritySettings.Value;
	}

	public static implicit operator PrioritySettings(int priority)
	{
		PrioritySettings result = default(PrioritySettings);
		result.Value = priority;
		result.Enabled = true;
		return result;
	}
}
