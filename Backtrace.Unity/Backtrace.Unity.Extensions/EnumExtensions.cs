using System;

namespace Backtrace.Unity.Extensions;

public static class EnumExtensions
{
	internal static bool HasFlag(this Enum variable, Enum value)
	{
		if (variable.GetType() != value.GetType())
		{
			throw new ArgumentException("The checked flag is not from the same type as the checked variable.");
		}
		switch (variable.GetTypeCode())
		{
		case TypeCode.SByte:
		case TypeCode.Int16:
		case TypeCode.Int32:
		case TypeCode.Int64:
			return (Convert.ToInt64(variable) & Convert.ToInt64(value)) != 0;
		case TypeCode.Byte:
		case TypeCode.UInt16:
		case TypeCode.UInt32:
		case TypeCode.UInt64:
			return (Convert.ToUInt64(variable) & Convert.ToUInt64(value)) != 0;
		default:
			return false;
		}
	}

	public static bool HasAllFlags<T>(this T rawSource)
	{
		Enum @enum = rawSource as Enum;
		foreach (object value in Enum.GetValues(typeof(T)))
		{
			if (!@enum.HasFlag(((T)value) as Enum))
			{
				return false;
			}
		}
		return true;
	}
}
