using System;

namespace UnityEngine.XR.Interaction.Toolkit.Feedback;

[Serializable]
public class HapticImpulseData
{
	[SerializeField]
	[Range(0f, 1f)]
	private float m_Amplitude;

	[SerializeField]
	private float m_Duration;

	[SerializeField]
	private float m_Frequency;

	public float amplitude
	{
		get
		{
			return m_Amplitude;
		}
		set
		{
			m_Amplitude = value;
		}
	}

	public float duration
	{
		get
		{
			return m_Duration;
		}
		set
		{
			m_Duration = value;
		}
	}

	public float frequency
	{
		get
		{
			return m_Frequency;
		}
		set
		{
			m_Frequency = value;
		}
	}
}
