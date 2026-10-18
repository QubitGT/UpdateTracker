using System;

namespace SouthPointe.Serialization.MessagePack;

internal static class CustomExtensions
{
	internal static bool IsNullable(this Type type)
	{
		if (type.IsValueType && Nullable.GetUnderlyingType(type) != null)
		{
			return true;
		}
		return false;
	}
}
