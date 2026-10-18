using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeFloat : EventAttributeValue<float>
{
	public EventAttributeFloat()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasFloat(id)), (Action<VFXEventAttribute, int, float>)delegate(VFXEventAttribute e, int id, float value)
		{
			e.SetFloat(id, value);
		})
	{
	}
}
