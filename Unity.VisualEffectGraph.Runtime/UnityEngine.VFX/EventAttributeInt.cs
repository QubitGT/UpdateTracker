using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeInt : EventAttributeValue<int>
{
	public EventAttributeInt()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasInt(id)), (Action<VFXEventAttribute, int, int>)delegate(VFXEventAttribute e, int id, int value)
		{
			e.SetInt(id, value);
		})
	{
	}
}
