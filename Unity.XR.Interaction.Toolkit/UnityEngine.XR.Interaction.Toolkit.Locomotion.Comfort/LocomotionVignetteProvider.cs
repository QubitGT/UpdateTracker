using System;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

[Serializable]
[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public class LocomotionVignetteProvider : ITunnelingVignetteProvider
{
	[SerializeField]
	private LocomotionProvider m_LocomotionProvider;

	[SerializeField]
	private bool m_Enabled;

	[SerializeField]
	private bool m_OverrideDefaultParameters;

	[SerializeField]
	private VignetteParameters m_OverrideParameters = new VignetteParameters();

	public LocomotionProvider locomotionProvider
	{
		get
		{
			return m_LocomotionProvider;
		}
		set
		{
			m_LocomotionProvider = value;
		}
	}

	public bool enabled
	{
		get
		{
			return m_Enabled;
		}
		set
		{
			m_Enabled = value;
		}
	}

	public bool overrideDefaultParameters
	{
		get
		{
			return m_OverrideDefaultParameters;
		}
		set
		{
			m_OverrideDefaultParameters = value;
		}
	}

	public VignetteParameters overrideParameters
	{
		get
		{
			return m_OverrideParameters;
		}
		set
		{
			m_OverrideParameters = value;
		}
	}

	public VignetteParameters vignetteParameters
	{
		get
		{
			if (!m_OverrideDefaultParameters)
			{
				return null;
			}
			return m_OverrideParameters;
		}
	}
}
