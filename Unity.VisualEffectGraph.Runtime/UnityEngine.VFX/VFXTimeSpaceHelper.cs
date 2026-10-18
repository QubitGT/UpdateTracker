using System;
using System.Collections.Generic;

namespace UnityEngine.VFX;

internal static class VFXTimeSpaceHelper
{
	public static IEnumerable<VisualEffectPlayableSerializedEvent> GetEventNormalizedSpace(PlayableTimeSpace space, VisualEffectControlPlayableBehaviour source)
	{
		return GetEventNormalizedSpace(space, source.events, source.clipStart, source.clipEnd);
	}

	private static IEnumerable<VisualEffectPlayableSerializedEvent> CollectClipEvents(VisualEffectControlClip source)
	{
		if (source.clipEvents == null)
		{
			yield break;
		}
		foreach (VisualEffectControlClip.ClipEvent clipEvent in source.clipEvents)
		{
			VisualEffectPlayableSerializedEvent visualEffectPlayableSerializedEvent = clipEvent.enter;
			VisualEffectPlayableSerializedEvent eventExit = clipEvent.exit;
			visualEffectPlayableSerializedEvent.editorColor = (eventExit.editorColor = clipEvent.editorColor);
			yield return visualEffectPlayableSerializedEvent;
			yield return eventExit;
		}
	}

	public static IEnumerable<VisualEffectPlayableSerializedEvent> GetEventNormalizedSpace(PlayableTimeSpace space, VisualEffectControlClip source, bool clipEvents)
	{
		IEnumerable<VisualEffectPlayableSerializedEvent> events = ((!clipEvents) ? source.singleEvents : CollectClipEvents(source));
		return GetEventNormalizedSpace(space, events, source.clipStart, source.clipEnd);
	}

	private static IEnumerable<VisualEffectPlayableSerializedEvent> GetEventNormalizedSpace(PlayableTimeSpace space, IEnumerable<VisualEffectPlayableSerializedEvent> events, double clipStart, double clipEnd)
	{
		foreach (VisualEffectPlayableSerializedEvent @event in events)
		{
			VisualEffectPlayableSerializedEvent visualEffectPlayableSerializedEvent = @event;
			visualEffectPlayableSerializedEvent.timeSpace = space;
			visualEffectPlayableSerializedEvent.time = GetTimeInSpace(@event.timeSpace, @event.time, space, clipStart, clipEnd);
			yield return visualEffectPlayableSerializedEvent;
		}
	}

	public static double GetTimeInSpace(PlayableTimeSpace srcSpace, double srcTime, PlayableTimeSpace dstSpace, double clipStart, double clipEnd)
	{
		if (srcSpace == dstSpace)
		{
			return srcTime;
		}
		switch (dstSpace)
		{
		case PlayableTimeSpace.AfterClipStart:
			switch (srcSpace)
			{
			case PlayableTimeSpace.BeforeClipEnd:
				return clipEnd - srcTime - clipStart;
			case PlayableTimeSpace.Percentage:
				return (clipEnd - clipStart) * (srcTime / 100.0);
			case PlayableTimeSpace.Absolute:
				return srcTime - clipStart;
			}
			break;
		case PlayableTimeSpace.BeforeClipEnd:
			switch (srcSpace)
			{
			case PlayableTimeSpace.AfterClipStart:
				return clipEnd - srcTime - clipStart;
			case PlayableTimeSpace.Percentage:
				return clipEnd - clipStart - (clipEnd - clipStart) * (srcTime / 100.0);
			case PlayableTimeSpace.Absolute:
				return clipEnd - srcTime;
			}
			break;
		case PlayableTimeSpace.Percentage:
			switch (srcSpace)
			{
			case PlayableTimeSpace.AfterClipStart:
				return 100.0 * srcTime / (clipEnd - clipStart);
			case PlayableTimeSpace.BeforeClipEnd:
				return 100.0 * (clipEnd - srcTime - clipStart) / (clipEnd - clipStart);
			case PlayableTimeSpace.Absolute:
				return 100.0 * (srcTime - clipStart) / (clipEnd - clipStart);
			}
			break;
		case PlayableTimeSpace.Absolute:
			switch (srcSpace)
			{
			case PlayableTimeSpace.AfterClipStart:
				return clipStart + srcTime;
			case PlayableTimeSpace.BeforeClipEnd:
				return clipEnd - srcTime;
			case PlayableTimeSpace.Percentage:
				return clipStart + (clipEnd - clipStart) * (srcTime / 100.0);
			}
			break;
		}
		throw new NotImplementedException(srcSpace.ToString() + " to " + dstSpace);
	}
}
