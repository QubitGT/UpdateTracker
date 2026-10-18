using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeUInt : EventAttributeValue<uint>
{
	public EventAttributeUInt()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasUint(id)), (Action<VFXEventAttribute, int, uint>)delegate(VFXEventAttribute e, int id, uint value)
		{
			e.SetUint(id, value);
		})
	{
	}
}
