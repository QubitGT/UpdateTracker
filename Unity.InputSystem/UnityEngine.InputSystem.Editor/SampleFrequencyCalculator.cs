using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.InputSystem.Editor;

internal struct SampleFrequencyCalculator
{
	private double m_LastUpdateTime;

	private int m_SampleCount;

	public float targetFrequency { get; private set; }

	public float frequency { get; private set; }

	public SampleFrequencyCalculator(float targetFrequency, double realtimeSinceStartup)
	{
		this.targetFrequency = targetFrequency;
		m_SampleCount = 0;
		frequency = 0f;
		m_LastUpdateTime = realtimeSinceStartup;
	}

	public void ProcessSample(InputEventPtr eventPtr)
	{
		if (eventPtr != null)
		{
			m_SampleCount++;
		}
	}

	public bool Update()
	{
		return Update(Time.realtimeSinceStartupAsDouble);
	}

	public bool Update(double realtimeSinceStartup)
	{
		double num = realtimeSinceStartup - m_LastUpdateTime;
		if (num < 1.0)
		{
			return false;
		}
		m_LastUpdateTime = realtimeSinceStartup;
		frequency = (float)((double)m_SampleCount / num);
		m_SampleCount = 0;
		return true;
	}
}
