using System;

namespace UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public struct InteractionState
{
	[Range(0f, 1f)]
	[SerializeField]
	private float m_Value;

	[SerializeField]
	private bool m_Active;

	private bool m_ActivatedThisFrame;

	private bool m_DeactivatedThisFrame;

	public float value
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_Active = value;
		}
	}

	public bool activatedThisFrame
	{
		get
		{
			return m_ActivatedThisFrame;
		}
		set
		{
			m_ActivatedThisFrame = value;
		}
	}

	public bool deactivatedThisFrame
	{
		get
		{
			return m_DeactivatedThisFrame;
		}
		set
		{
			m_DeactivatedThisFrame = value;
		}
	}

	[Obsolete("deActivatedThisFrame has been deprecated. Use deactivatedThisFrame instead. (UnityUpgradable) -> deactivatedThisFrame", true)]
	public bool deActivatedThisFrame
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public void SetFrameState(bool isActive)
	{
		SetFrameState(isActive, isActive ? 1f : 0f);
	}

	public void SetFrameState(bool isActive, float newValue)
	{
		value = newValue;
		activatedThisFrame = !active && isActive;
		deactivatedThisFrame = active && !isActive;
		active = isActive;
	}

	public void SetFrameDependent(bool wasActive)
	{
		activatedThisFrame = !wasActive && active;
		deactivatedThisFrame = wasActive && !active;
	}

	public void ResetFrameDependent()
	{
		activatedThisFrame = false;
		deactivatedThisFrame = false;
	}

	[Obsolete("Reset has been renamed. Use ResetFrameDependent instead. (UnityUpgradable) -> ResetFrameDependent()", true)]
	public void Reset()
	{
	}
}
