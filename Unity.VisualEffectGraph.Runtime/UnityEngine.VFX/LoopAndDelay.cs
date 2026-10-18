namespace UnityEngine.VFX;

internal class LoopAndDelay : VFXSpawnerCallbacks
{
	public class InputProperties
	{
		[Tooltip("Number of Loops (< 0 for infinite), evaluated when Context Start is hit")]
		public int LoopCount = 1;

		[Tooltip("Duration of one loop, evaluated every loop")]
		public float LoopDuration = 4f;

		[Tooltip("Duration of in-between delay (after each loop), evaluated every loop")]
		public float Delay = 1f;
	}

	private int m_LoopMaxCount;

	private int m_LoopCurrentIndex;

	private float m_WaitingForTotalTime;

	private static readonly int loopCountPropertyID = Shader.PropertyToID("LoopCount");

	private static readonly int loopDurationPropertyID = Shader.PropertyToID("LoopDuration");

	private static readonly int delayPropertyID = Shader.PropertyToID("Delay");

	public sealed override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		m_LoopMaxCount = vfxValues.GetInt(loopCountPropertyID);
		m_WaitingForTotalTime = vfxValues.GetFloat(loopDurationPropertyID);
		m_LoopCurrentIndex = 0;
		if (m_LoopMaxCount == m_LoopCurrentIndex)
		{
			state.playing = false;
		}
	}

	public sealed override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		if (m_LoopCurrentIndex != m_LoopMaxCount && state.totalTime > m_WaitingForTotalTime)
		{
			if (state.playing)
			{
				m_WaitingForTotalTime = state.totalTime + vfxValues.GetFloat(delayPropertyID);
				state.playing = false;
				m_LoopCurrentIndex = ((m_LoopCurrentIndex + 1 > 0) ? (m_LoopCurrentIndex + 1) : 0);
			}
			else
			{
				m_WaitingForTotalTime = vfxValues.GetFloat(loopDurationPropertyID);
				state.totalTime = 0f;
				state.playing = true;
			}
		}
	}

	public sealed override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
	{
		m_LoopCurrentIndex = m_LoopMaxCount;
	}
}
