using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
internal class VisualEffectActivationClip : PlayableAsset, ITimelineClipAsset
{
	public VisualEffectActivationBehaviour activationBehavior = new VisualEffectActivationBehaviour();

	public ClipCaps clipCaps => ClipCaps.None;

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<VisualEffectActivationBehaviour> scriptPlayable = ScriptPlayable<VisualEffectActivationBehaviour>.Create(graph, activationBehavior);
		scriptPlayable.GetBehaviour();
		return scriptPlayable;
	}
}
