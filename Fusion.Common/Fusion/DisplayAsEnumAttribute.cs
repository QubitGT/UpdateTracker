using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public class DisplayAsEnumAttribute : DrawerPropertyAttribute
{
	public Type EnumType { get; }

	public string EnumTypeMemberName { get; }

	public DisplayAsEnumAttribute(Type enumType)
	{
		EnumType = enumType;
	}

	public DisplayAsEnumAttribute(string enumTypeMemberName)
	{
		EnumTypeMemberName = enumTypeMemberName;
	}
}
