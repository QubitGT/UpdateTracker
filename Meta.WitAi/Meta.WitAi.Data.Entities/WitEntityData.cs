using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.WitAi.Data.Entities;

public class WitEntityData : WitEntityDataBase<string>
{
	[Preserve]
	public WitEntityData()
	{
	}

	[Preserve]
	public WitEntityData(WitResponseNode node)
	{
		FromEntityWitResponseNode(node);
	}

	public static implicit operator bool(WitEntityData data)
	{
		if (null != data)
		{
			return !string.IsNullOrEmpty(data.value);
		}
		return false;
	}

	public static implicit operator string(WitEntityData data)
	{
		return data.value;
	}

	public static bool operator ==(WitEntityData data, object value)
	{
		return object.Equals(data?.value, value);
	}

	public static bool operator !=(WitEntityData data, object value)
	{
		return !object.Equals(data?.value, value);
	}

	public override bool Equals(object obj)
	{
		if (obj is string text)
		{
			return text == value;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
