using System;

namespace UnityEngine.Animations.Rigging;

[Obsolete("PropertyUtils is deprecated. Use ConstraintsUtils instead. (UnityUpgradable) -> ConstraintsUtils")]
public static class PropertyUtils
{
	[Obsolete("ConstructConstraintDataPropertyName is deprecated. Use ConstraintsUtils.ConstructConstraintDataPropertyName instead.")]
	public static string ConstructConstraintDataPropertyName(string property)
	{
		return ConstraintsUtils.ConstructConstraintDataPropertyName(property);
	}

	[Obsolete("ConstructCustomPropertyName is deprecated. Use ConstraintsUtils.ConstructCustomPropertyName instead.")]
	public static string ConstructCustomPropertyName(Component component, string property)
	{
		return ConstraintsUtils.ConstructCustomPropertyName(component, property);
	}
}
