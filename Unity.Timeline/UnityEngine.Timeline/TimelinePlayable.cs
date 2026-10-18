using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline;

public class TimelinePlayable : PlayableBehaviour
{
	private readonly struct TrackCacheManager : IDisposable
	{
		public readonly HashSet<AnimationTrack> trackCache;

		public TrackCacheManager(HashSet<AnimationTrack> cache, IReadOnlyList<RuntimeElement> activeRuntimeElements)
		{
			trackCache = cache;
			GetTrackAssetsFromRuntimeElements(activeRuntimeElements);
		}

		public void Dispose()
		{
			trackCache.Clear();
		}

		private void GetTrackAssetsFromRuntimeElements(IReadOnlyList<RuntimeElement> activeRuntimeElements)
		{
			for (int i = 0; i < activeRuntimeElements.Count; i++)
			{
				if (activeRuntimeElements[i] is RuntimeClip runtimeClip && runtimeClip.clip?.GetParentTrack() is AnimationTrack item)
				{
					trackCache.Add(item);
				}
			}
		}
	}

	private static ProfilerMarker k_CreateTimelineGraphMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Timeline.CreatePlayableGraph");

	private static ProfilerMarker k_CreateTimelineTrackMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Timeline.CreateTrackPlayable");

	private static ProfilerMarker k_CreateTimelineTrackOutputsMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Timeline.CreateTrackPlayableOutputs");

	private static ProfilerMarker m_findActiveClipsMarker = new ProfilerMarker(ProfilerCategory.Scripts, "TimelinePlayable.GetActiveClips");

	private static ProfilerMarker m_SetClipsLocalTimeMarker = new ProfilerMarker(ProfilerCategory.Scripts, "TimelinePlayable.SetActiveClipsTime");

	private IntervalTree<RuntimeElement> m_IntervalTree = new IntervalTree<RuntimeElement>();

	private List<RuntimeElement> m_ActiveClips = new List<RuntimeElement>();

	private List<RuntimeElement> m_CurrentListOfActiveClips;

	private int m_ActiveBit;

	private Dictionary<TrackAsset, Playable> m_PlayableCache = new Dictionary<TrackAsset, Playable>();

	internal static bool muteAudioScrubbing = true;

	private readonly Dictionary<AnimationTrack, List<ITimelineEvaluateCallback>> m_EvaluateCallbacks = new Dictionary<AnimationTrack, List<ITimelineEvaluateCallback>>();

	private readonly List<ITimelineEvaluateCallback> m_AlwaysEvaluateCallbacks = new List<ITimelineEvaluateCallback>();

	private readonly HashSet<ITimelineEvaluateCallback> m_ForceEvaluateNextEvaluate = new HashSet<ITimelineEvaluateCallback>();

	private readonly HashSet<ITimelineEvaluateCallback> m_InvokedThisFrame = new HashSet<ITimelineEvaluateCallback>();

	private readonly HashSet<AnimationTrack> m_ActiveTracksToEvaluateCache = new HashSet<AnimationTrack>();

	public static ScriptPlayable<TimelinePlayable> Create(PlayableGraph graph, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
	{
		if (tracks == null)
		{
			throw new ArgumentNullException("Tracks list is null", "tracks");
		}
		if (go == null)
		{
			throw new ArgumentNullException("GameObject parameter is null", "go");
		}
		ScriptPlayable<TimelinePlayable> scriptPlayable = ScriptPlayable<TimelinePlayable>.Create(graph);
		scriptPlayable.SetTraversalMode(PlayableTraversalMode.Passthrough);
		scriptPlayable.GetBehaviour().Compile(graph, scriptPlayable, tracks, go, autoRebalance, createOutputs);
		return scriptPlayable;
	}

	public void Compile(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
	{
		if (tracks == null)
		{
			throw new ArgumentNullException("Tracks list is null", "tracks");
		}
		if (go == null)
		{
			throw new ArgumentNullException("GameObject parameter is null", "go");
		}
		List<TrackAsset> list = new List<TrackAsset>(tracks);
		int capacity = list.Count * 2 + list.Count;
		m_CurrentListOfActiveClips = new List<RuntimeElement>(capacity);
		m_ActiveClips = new List<RuntimeElement>(capacity);
		m_EvaluateCallbacks.Clear();
		m_AlwaysEvaluateCallbacks.Clear();
		m_PlayableCache.Clear();
		CompileTrackList(graph, timelinePlayable, list, go, createOutputs);
	}

	private void CompileTrackList(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool createOutputs)
	{
		foreach (TrackAsset track in tracks)
		{
			if (track.IsCompilable() && !m_PlayableCache.ContainsKey(track))
			{
				track.SortClips();
				track.ComputeBlendsFromOverlaps();
				CreateTrackPlayable(graph, timelinePlayable, track, go, createOutputs);
			}
		}
	}

	private void CreateTrackOutput(PlayableGraph graph, TrackAsset track, GameObject go, Playable playable, int port)
	{
		if (track.isSubTrack)
		{
			return;
		}
		foreach (PlayableBinding output in track.outputs)
		{
			PlayableOutput playableOutput = output.CreateOutput(graph);
			playableOutput.SetReferenceObject(output.sourceObject);
			playableOutput.SetSourcePlayable(playable, port);
			playableOutput.SetWeight(1f);
			if (track is AnimationTrack track2)
			{
				AddPlayableOutputCallbacks(track2, playableOutput);
			}
			if (playableOutput.IsPlayableOutputOfType<AudioPlayableOutput>())
			{
				((AudioPlayableOutput)playableOutput).SetEvaluateOnSeek(!muteAudioScrubbing);
			}
			if (track.timelineAsset.markerTrack == track)
			{
				PlayableDirector component = go.GetComponent<PlayableDirector>();
				playableOutput.SetUserData(component);
				INotificationReceiver[] components = go.GetComponents<INotificationReceiver>();
				foreach (INotificationReceiver receiver in components)
				{
					playableOutput.AddNotificationReceiver(receiver);
				}
			}
		}
	}

	private Playable CreateTrackPlayable(PlayableGraph graph, Playable timelinePlayable, TrackAsset track, GameObject go, bool createOutputs)
	{
		if (!track.IsCompilable())
		{
			return timelinePlayable;
		}
		if (m_PlayableCache.TryGetValue(track, out var value))
		{
			return value;
		}
		if (track.name == "root")
		{
			return timelinePlayable;
		}
		TrackAsset trackAsset = track.parent as TrackAsset;
		Playable playable = ((trackAsset != null) ? CreateTrackPlayable(graph, timelinePlayable, trackAsset, go, createOutputs) : timelinePlayable);
		Playable playable2 = track.CreatePlayableGraph(graph, go, m_IntervalTree, timelinePlayable);
		bool flag = false;
		if (!playable2.IsValid())
		{
			throw new InvalidOperationException(track.name + "(" + track.GetType()?.ToString() + ") did not produce a valid playable.");
		}
		if (playable.IsValid() && playable2.IsValid())
		{
			int inputCount = playable.GetInputCount();
			playable.SetInputCount(inputCount + 1);
			flag = graph.Connect(playable2, 0, playable, inputCount);
			playable.SetInputWeight(inputCount, 1f);
		}
		if (createOutputs && flag)
		{
			CreateTrackOutput(graph, track, go, playable, playable.GetInputCount() - 1);
		}
		CacheTrack(track, playable2);
		return playable2;
	}

	public override void PrepareFrame(Playable playable, FrameData info)
	{
		Evaluate(playable, info);
	}

	private void Evaluate(Playable playable, FrameData frameData)
	{
		if (m_IntervalTree == null)
		{
			return;
		}
		double time = playable.GetTime();
		m_ActiveBit = ((m_ActiveBit == 0) ? 1 : 0);
		m_CurrentListOfActiveClips.Clear();
		m_IntervalTree.IntersectsWith(DiscreteTime.GetNearestTick(time), m_CurrentListOfActiveClips);
		foreach (RuntimeElement currentListOfActiveClip in m_CurrentListOfActiveClips)
		{
			currentListOfActiveClip.intervalBit = m_ActiveBit;
		}
		double rootDuration = (double)new DiscreteTime(playable.GetDuration());
		foreach (RuntimeElement activeClip in m_ActiveClips)
		{
			if (activeClip.intervalBit != m_ActiveBit)
			{
				activeClip.DisableAt(time, rootDuration, frameData);
			}
		}
		m_ActiveClips.Clear();
		for (int i = 0; i < m_CurrentListOfActiveClips.Count; i++)
		{
			m_CurrentListOfActiveClips[i].EvaluateAt(time, frameData);
			m_ActiveClips.Add(m_CurrentListOfActiveClips[i]);
		}
		InvokeOutputCallbacks(m_CurrentListOfActiveClips);
	}

	private void CacheTrack(TrackAsset track, Playable playable)
	{
		m_PlayableCache[track] = playable;
	}

	private static void ForAOTCompilationOnly()
	{
		new List<IntervalTree<RuntimeElement>.Entry>();
	}

	private void AddPlayableOutputCallbacks(AnimationTrack track, PlayableOutput playableOutput)
	{
		AddOutputWeightProcessor(track, (AnimationPlayableOutput)playableOutput);
	}

	private void AddOutputWeightProcessor(AnimationTrack track, AnimationPlayableOutput animOutput)
	{
		AnimationOutputWeightProcessor animationOutputWeightProcessor = new AnimationOutputWeightProcessor(animOutput);
		if (track.inClipMode)
		{
			AddEvaluateCallback(track, animationOutputWeightProcessor);
		}
		else
		{
			m_AlwaysEvaluateCallbacks.Add(animationOutputWeightProcessor);
		}
		m_ForceEvaluateNextEvaluate.Add(animationOutputWeightProcessor);
	}

	private void AddEvaluateCallback(AnimationTrack track, ITimelineEvaluateCallback callback)
	{
		if (m_EvaluateCallbacks.TryGetValue(track, out var value))
		{
			value.Add(callback);
			return;
		}
		m_EvaluateCallbacks[track] = new List<ITimelineEvaluateCallback> { callback };
	}

	private void InvokeOutputCallbacks(IReadOnlyList<RuntimeElement> activeRuntimeElements)
	{
		foreach (ITimelineEvaluateCallback item in m_ForceEvaluateNextEvaluate)
		{
			item.Evaluate();
			m_InvokedThisFrame.Add(item);
		}
		m_ForceEvaluateNextEvaluate.Clear();
		if (activeRuntimeElements.Count > 0)
		{
			TrackCacheManager trackCacheManager = new TrackCacheManager(m_ActiveTracksToEvaluateCache, activeRuntimeElements);
			try
			{
				foreach (AnimationTrack item2 in trackCacheManager.trackCache)
				{
					if (!TryGetCallbackList(item2, out var list))
					{
						continue;
					}
					foreach (ITimelineEvaluateCallback item3 in list)
					{
						if (!m_InvokedThisFrame.Contains(item3))
						{
							item3.Evaluate();
							m_InvokedThisFrame.Add(item3);
							m_ForceEvaluateNextEvaluate.Add(item3);
						}
					}
				}
			}
			finally
			{
				((IDisposable)trackCacheManager).Dispose();
			}
		}
		else
		{
			foreach (List<ITimelineEvaluateCallback> value in m_EvaluateCallbacks.Values)
			{
				foreach (ITimelineEvaluateCallback item4 in value)
				{
					if (!m_InvokedThisFrame.Contains(item4))
					{
						item4.Evaluate();
					}
				}
			}
		}
		foreach (ITimelineEvaluateCallback alwaysEvaluateCallback in m_AlwaysEvaluateCallbacks)
		{
			alwaysEvaluateCallback.Evaluate();
		}
		m_InvokedThisFrame.Clear();
	}

	private bool TryGetCallbackList(AnimationTrack track, out List<ITimelineEvaluateCallback> list)
	{
		if (track == null)
		{
			list = null;
			return false;
		}
		if (m_EvaluateCallbacks.TryGetValue(track, out list))
		{
			return true;
		}
		return TryGetCallbackList(track.parent as AnimationTrack, out list);
	}
}
