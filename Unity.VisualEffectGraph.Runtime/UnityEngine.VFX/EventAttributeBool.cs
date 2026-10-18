using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeBool : EventAttributeValue<bool>
{
	public EventAttributeBool()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasBool(id)), (Action<VFXEventAttribute, int, bool>)delegate(VFXEventAttribute e, int id, bool value)
		{
			e.SetBool(id, value);
		})
	{
	}
}
