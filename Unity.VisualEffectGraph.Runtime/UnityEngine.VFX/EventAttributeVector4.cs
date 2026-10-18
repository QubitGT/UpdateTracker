using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeVector4 : EventAttributeValue<Vector4>
{
	public EventAttributeVector4()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasVector4(id)), (Action<VFXEventAttribute, int, Vector4>)delegate(VFXEventAttribute e, int id, Vector4 value)
		{
			e.SetVector4(id, value);
		})
	{
	}
}
