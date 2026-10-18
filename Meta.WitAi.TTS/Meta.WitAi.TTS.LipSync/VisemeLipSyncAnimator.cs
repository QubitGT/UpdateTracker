using System;
using Meta.WitAi.Attributes;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.LipSync;

public class VisemeLipSyncAnimator : TTSEventAnimator<TTSVisemeEvent, Viseme>, IVisemeAnimatorProvider
{
	[Header("Viseme Events")]
	[TooltipBox("Fired when entering or passing a sample with this specified viseme")]
	[SerializeField]
	private VisemeChangedEvent _onVisemeStarted = new VisemeChangedEvent();

	[TooltipBox("Fired when entering or passing a new sample with a different specified viseme")]
	[SerializeField]
	private VisemeChangedEvent _onVisemeFinished = new VisemeChangedEvent();

	[TooltipBox("Fired once per frame with the previous viseme and next viseme as well as a percentage of the current frame in between each viseme.")]
	[SerializeField]
	[FormerlySerializedAs("onVisemeLerp")]
	private VisemeLerpEvent _onVisemeLerp = new VisemeLerpEvent();

	public Viseme LastViseme { get; private set; }

	public VisemeChangedEvent OnVisemeStarted => _onVisemeStarted;

	public VisemeChangedEvent OnVisemeFinished => _onVisemeFinished;

	public VisemeLerpEvent OnVisemeLerp => _onVisemeLerp;

	[Obsolete("Use OnVisemeStarted, OnVisemeLerp or OnVisemeFinished instead.")]
	public VisemeChangedEvent OnVisemeChanged => OnVisemeStarted;

	protected override void LerpEvent(TTSVisemeEvent fromEvent, TTSVisemeEvent toEvent, float percentage)
	{
		SetViseme((percentage >= 1f) ? toEvent.Data : fromEvent.Data);
		percentage = Mathf.Clamp01(percentage);
		OnVisemeLerp?.Invoke(fromEvent.Data, toEvent.Data, percentage);
	}

	private void SetViseme(Viseme newViseme)
	{
		if (LastViseme != newViseme)
		{
			Viseme lastViseme = LastViseme;
			LastViseme = newViseme;
			OnVisemeFinished?.Invoke(lastViseme);
			OnVisemeStarted?.Invoke(LastViseme);
		}
	}
}
