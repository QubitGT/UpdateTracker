using System;
using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.WitAi.Data.Entities;

public class WitEntityFloatData : WitEntityDataBase<float>
{
	[Preserve]
	public WitEntityFloatData()
	{
	}

	[Preserve]
	public WitEntityFloatData(WitResponseNode node)
	{
		FromEntityWitResponseNode(node);
	}

	public static implicit operator bool(WitEntityFloatData data)
	{
		return data?.hasData ?? false;
	}

	public bool Approximately(float v, float tolerance = 0.001f)
	{
		return Math.Abs(v - value) < tolerance;
	}

	public static bool operator ==(WitEntityFloatData data, float value)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == value;
	}

	public static bool operator !=(WitEntityFloatData data, float value)
	{
		return !(data == value);
	}

	public static bool operator ==(WitEntityFloatData data, int value)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == (float)value;
	}

	public static bool operator !=(WitEntityFloatData data, int value)
	{
		return !(data == value);
	}

	public static bool operator ==(float value, WitEntityFloatData data)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == value;
	}

	public static bool operator !=(float value, WitEntityFloatData data)
	{
		return !(data == value);
	}

	public static bool operator ==(int value, WitEntityFloatData data)
	{
		if (data == null)
		{
			return false;
		}
		return data.value == (float)value;
	}

	public static bool operator !=(int value, WitEntityFloatData data)
	{
		return !(data == value);
	}

	public override bool Equals(object obj)
	{
		if (obj is float num)
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
