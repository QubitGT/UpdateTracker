using System;

namespace UnityEngine.VFX;

[Serializable]
internal struct EventAttributes
{
	[SerializeReference]
	public EventAttribute[] content;
}
