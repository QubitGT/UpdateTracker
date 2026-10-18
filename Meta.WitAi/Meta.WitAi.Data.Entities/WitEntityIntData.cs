using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.WitAi.Data.Entities;

public class WitEntityIntData : WitEntityDataBase<int>
{
	[Preserve]
	public WitEntityIntData()
	{
	}

	[Preserve]
	public WitEntityIntData(WitResponseNode node)
	{
		FromEntityWitResponseNode(node);
	}

	public static implicit operator bool(WitEntityIntData data)
	{
		return data?.hasData ?? false;
	}

	public static bool operator ==(WitEntityIntData data, int value)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == value;
	}

	public static bool operator !=(WitEntityIntData data, int value)
	{
		return !(data == value);
	}

	public static bool operator ==(int value, WitEntityIntData data)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == value;
	}

	public static bool operator !=(int value, WitEntityIntData data)
	{
		return !(data == value);
	}

	public override bool Equals(object obj)
	{
		if (obj is int num)
		{
			return num == value;
		}
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
