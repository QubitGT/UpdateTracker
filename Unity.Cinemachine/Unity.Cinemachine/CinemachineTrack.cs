using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.Cinemachine;

[Serializable]
[TrackClipType(typeof(CinemachineShot))]
[TrackBindingType(typeof(CinemachineBrain), TrackBindingFlags.None)]
[TrackColor(0.53f, 0f, 0.08f)]
public class CinemachineTrack : TrackAsset
{
	[Tooltip("The priority controls the precedence that this track takes over other CinemachineTracks.  Tracks with higher priority will override tracks with lower priority.  If two simultaneous tracks have the same priority, then the more-recently instanced track will take precedence.  Track priority is unrelated to Cinemachine Camera priority.")]
	public int TrackPriority;

	public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
	{
		ScriptPlayable<CinemachinePlayableMixer> scriptPlayable = ScriptPlayable<CinemachinePlayableMixer>.Create(graph);
		scriptPlayable.SetInputCount(inputCount);
		scriptPlayable.GetBehaviour().Priority = TrackPriority;
		return scriptPlayable;
	}
}
