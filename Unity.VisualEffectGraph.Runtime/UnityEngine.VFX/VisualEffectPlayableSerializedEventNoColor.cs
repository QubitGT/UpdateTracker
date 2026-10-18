using System;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX;

[Serializable]
internal struct VisualEffectPlayableSerializedEventNoColor
{
	public double time;

	public PlayableTimeSpace timeSpace;

	public ExposedProperty name;

	public EventAttributes eventAttributes;

	public static implicit operator VisualEffectPlayableSerializedEvent(VisualEffectPlayableSerializedEventNoColor evt)
	{
		VisualEffectPlayableSerializedEvent result = default(VisualEffectPlayableSerializedEvent);
		result.time = evt.time;
		result.timeSpace = evt.timeSpace;
		result.name = evt.name;
		result.eventAttributes = evt.eventAttributes;
		return result;
	}
}
