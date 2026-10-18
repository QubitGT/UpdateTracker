namespace UnityEngine.XR.OpenXR;

internal sealed class WaitForRestartFinish : CustomYieldInstruction
{
	private float m_Timeout;

	public override bool keepWaiting
	{
		get
		{
			if (!OpenXRRestarter.Instance.isRunning)
			{
				return false;
			}
			if (Time.realtimeSinceStartup > m_Timeout)
			{
				Debug.LogError("WaitForRestartFinish: Timeout");
				return false;
			}
			return true;
		}
	}

	public WaitForRestartFinish(float timeout = 5f)
	{
		m_Timeout = Time.realtimeSinceStartup + timeout;
	}
}
