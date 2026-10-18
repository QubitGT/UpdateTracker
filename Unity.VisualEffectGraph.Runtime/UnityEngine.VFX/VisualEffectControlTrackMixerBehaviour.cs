using UnityEngine.Playables;

namespace UnityEngine.VFX;

internal class VisualEffectControlTrackMixerBehaviour : PlayableBehaviour
{
	private VisualEffectControlTrackController m_ScrubbingCacheHelper;

	private VisualEffect m_Target;

	private bool m_ReinitWithBinding;

	private bool m_ReinitWithUnbinding;

	public void Init(VisualEffectControlTrack parentTrack, bool reinitWithBinding, bool reinitWithUnbinding)
	{
		m_ReinitWithBinding = reinitWithBinding;
		m_ReinitWithUnbinding = reinitWithUnbinding;
	}

	private void ApplyFrame(Playable playable, FrameData data)
	{
		if (!(m_Target == null))
		{
			if (m_ScrubbingCacheHelper == null)
			{
				m_ScrubbingCacheHelper = new VisualEffectControlTrackController();
				VisualEffectControlTrack parentTrack = null;
				m_ScrubbingCacheHelper.Init(playable, m_Target, parentTrack);
			}
			double duration = playable.GetOutput(0).GetDuration();
			double time = playable.GetTime();
			int num = (int)(time / duration);
			time -= (double)num * duration;
			float deltaTime = data.deltaTime;
			m_ScrubbingCacheHelper.Update(time, deltaTime);
		}
	}

	private void BindVFX(VisualEffect vfx)
	{
		m_Target = vfx;
		if (m_Target != null && m_ReinitWithBinding)
		{
			m_Target.Reinit(sendInitialEventAndPrewarm: false);
		}
	}

	private void UnbindVFX()
	{
		if (m_Target != null && m_ReinitWithUnbinding)
		{
			m_Target.Reinit(sendInitialEventAndPrewarm: true);
		}
		m_Target = null;
	}

	public override void PrepareFrame(Playable playable, FrameData data)
	{
		VisualEffect visualEffect = data.output.GetUserData() as VisualEffect;
		if (m_Target != visualEffect)
		{
			UnbindVFX();
			if (visualEffect != null)
			{
				if (visualEffect.visualEffectAsset == null)
				{
					visualEffect = null;
				}
				else if (!visualEffect.isActiveAndEnabled)
				{
					visualEffect = null;
				}
			}
			BindVFX(visualEffect);
			InvalidateScrubbingHelper();
		}
		ApplyFrame(playable, data);
	}

	public override void OnBehaviourPause(Playable playable, FrameData data)
	{
		base.OnBehaviourPause(playable, data);
		ApplyFrame(playable, data);
	}

	private void InvalidateScrubbingHelper()
	{
		if (m_ScrubbingCacheHelper != null)
		{
			m_ScrubbingCacheHelper.Release();
			m_ScrubbingCacheHelper = null;
		}
	}

	public override void OnPlayableCreate(Playable playable)
	{
		InvalidateScrubbingHelper();
	}

	public override void OnPlayableDestroy(Playable playable)
	{
		InvalidateScrubbingHelper();
		UnbindVFX();
	}
}
