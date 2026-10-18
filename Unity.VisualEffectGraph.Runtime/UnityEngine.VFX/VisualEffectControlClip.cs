using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX;

[Serializable]
internal class VisualEffectControlClip : PlayableAsset, ITimelineClipAsset
{
	public enum ReinitMode
	{
		None,
		OnExitClip,
		OnEnterClip,
		OnEnterOrExitClip
	}

	[Serializable]
	public struct PrewarmClipSettings
	{
		public bool enable;

		public uint stepCount;

		public float deltaTime;

		public ExposedProperty eventName;
	}

	[Serializable]
	public struct ClipEvent
	{
		public static Color defaultEditorColor = new Color32(123, 158, 5, byte.MaxValue);

		public Color editorColor;

		public VisualEffectPlayableSerializedEventNoColor enter;

		public VisualEffectPlayableSerializedEventNoColor exit;
	}

	[NotKeyable]
	public bool scrubbing = true;

	[NotKeyable]
	public uint startSeed;

	[NotKeyable]
	public ReinitMode reinit = ReinitMode.OnEnterOrExitClip;

	[NotKeyable]
	public PrewarmClipSettings prewarm = new PrewarmClipSettings
	{
		enable = false,
		stepCount = 20u,
		deltaTime = 0.05f,
		eventName = "OnPlay"
	};

	[NotKeyable]
	public List<ClipEvent> clipEvents = new List<ClipEvent>
	{
		new ClipEvent
		{
			editorColor = ClipEvent.defaultEditorColor,
			enter = new VisualEffectPlayableSerializedEventNoColor
			{
				name = "OnPlay",
				time = 0.0,
				timeSpace = PlayableTimeSpace.AfterClipStart,
				eventAttributes = new EventAttributes
				{
					content = Array.Empty<EventAttribute>()
				}
			},
			exit = new VisualEffectPlayableSerializedEventNoColor
			{
				name = "OnStop",
				time = 0.0,
				timeSpace = PlayableTimeSpace.BeforeClipEnd,
				eventAttributes = new EventAttributes
				{
					content = Array.Empty<EventAttribute>()
				}
			}
		}
	};

	[NotKeyable]
	public List<VisualEffectPlayableSerializedEvent> singleEvents = new List<VisualEffectPlayableSerializedEvent>();

	public ClipCaps clipCaps => ClipCaps.None;

	public double clipStart { get; set; }

	public double clipEnd { get; set; }

	public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
	{
		ScriptPlayable<VisualEffectControlPlayableBehaviour> scriptPlayable = ScriptPlayable<VisualEffectControlPlayableBehaviour>.Create(graph);
		VisualEffectControlPlayableBehaviour behaviour = scriptPlayable.GetBehaviour();
		behaviour.clipStart = clipStart;
		behaviour.clipEnd = clipEnd;
		behaviour.scrubbing = scrubbing;
		behaviour.startSeed = startSeed;
		if (scrubbing)
		{
			behaviour.reinitEnter = true;
			behaviour.reinitExit = true;
		}
		else
		{
			switch (reinit)
			{
			case ReinitMode.None:
				behaviour.reinitEnter = false;
				behaviour.reinitExit = false;
				break;
			case ReinitMode.OnExitClip:
				behaviour.reinitEnter = false;
				behaviour.reinitExit = true;
				break;
			case ReinitMode.OnEnterClip:
				behaviour.reinitEnter = true;
				behaviour.reinitExit = false;
				break;
			case ReinitMode.OnEnterOrExitClip:
				behaviour.reinitEnter = true;
				behaviour.reinitExit = true;
				break;
			}
		}
		if (clipEvents == null)
		{
			clipEvents = new List<ClipEvent>();
		}
		if (singleEvents == null)
		{
			singleEvents = new List<VisualEffectPlayableSerializedEvent>();
		}
		behaviour.clipEventsCount = (uint)clipEvents.Count;
		List<VisualEffectPlayableSerializedEvent> list = new List<VisualEffectPlayableSerializedEvent>();
		foreach (ClipEvent clipEvent in clipEvents)
		{
			list.Add(clipEvent.enter);
			list.Add(clipEvent.exit);
		}
		foreach (VisualEffectPlayableSerializedEvent singleEvent in singleEvents)
		{
			list.Add(singleEvent);
		}
		behaviour.events = list.ToArray();
		if (!prewarm.enable || !behaviour.reinitEnter || prewarm.eventName == null || string.IsNullOrEmpty((string)prewarm.eventName))
		{
			behaviour.prewarmStepCount = 0u;
			behaviour.prewarmDeltaTime = 0f;
			behaviour.prewarmEvent = null;
		}
		else
		{
			behaviour.prewarmStepCount = prewarm.stepCount;
			behaviour.prewarmDeltaTime = prewarm.deltaTime;
			behaviour.prewarmEvent = prewarm.eventName;
		}
		return scriptPlayable;
	}
}
