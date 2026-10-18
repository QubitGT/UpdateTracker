using System;

namespace UnityEngine.VFX;

[Serializable]
internal class EventAttributeVector3 : EventAttributeValue<Vector3>
{
	public EventAttributeVector3()
		: base((Func<VFXEventAttribute, int, bool>)((VFXEventAttribute e, int id) => e.HasVector3(id)), (Action<VFXEventAttribute, int, Vector3>)delegate(VFXEventAttribute e, int id, Vector3 value)
		{
			e.SetVector3(id, value);
		})
	{
	}
}
