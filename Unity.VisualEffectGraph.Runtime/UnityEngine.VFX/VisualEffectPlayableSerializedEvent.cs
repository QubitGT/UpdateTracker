using System;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX;

[Serializable]
internal struct VisualEffectPlayableSerializedEvent
{
	public Color editorColor;

	public double time;

	public PlayableTimeSpace timeSpace;

	public ExposedProperty name;

	public EventAttributes eventAttributes;
}
