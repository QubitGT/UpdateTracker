using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.VFX;

[TrackColor(0.5990566f, 0.9038978f, 1f)]
[TrackClipType(typeof(VisualEffectControlClip))]
[TrackBindingType(typeof(VisualEffect))]
internal class VisualEffectControlTrack : TrackAsset
{
	public enum ReinitMode
	{
		None,
		OnBindingEnable,
		OnBindingDisable,
		OnBindingEnableOrDisable
	}

	private const int kCurrentVersion = 1;

	[SerializeField]
	[HideInInspector]
	private int m_VFXVersion;

	[SerializeField]
	[NotKeyable]
	public ReinitMode reinit = ReinitMode.OnBindingEnableOrDisable;

	public bool IsUpToDate()
	{
		return m_VFXVersion == 1;
	}

	protected override void OnBeforeTrackSerialize()
	{
		base.OnBeforeTrackSerialize();
		if (GetClips().All((TimelineClip x) => x.asset is VisualEffectControlClip))
		{
			m_VFXVersion = 1;
		}
	}

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		foreach (TimelineClip clip in GetClips())
		{
			if (clip.asset is VisualEffectControlClip visualEffectControlClip)
			{
				visualEffectControlClip.clipStart = clip.start;
				visualEffectControlClip.clipEnd = clip.end;
			}
		}
		ScriptPlayable<VisualEffectControlTrackMixerBehaviour> scriptPlayable = ScriptPlayable<VisualEffectControlTrackMixerBehaviour>.Create(graph, inputCount);
		VisualEffectControlTrackMixerBehaviour behaviour = scriptPlayable.GetBehaviour();
		bool reinitWithBinding = reinit == ReinitMode.OnBindingEnable || reinit == ReinitMode.OnBindingEnableOrDisable;
		bool reinitWithUnbinding = reinit == ReinitMode.OnBindingDisable || reinit == ReinitMode.OnBindingEnableOrDisable;
		behaviour.Init(this, reinitWithBinding, reinitWithUnbinding);
		return scriptPlayable;
	}

	public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
	{
		if (director.GetGenericBinding(this) is VisualEffect)
		{
			base.GatherProperties(director, driver);
		}
	}
}
