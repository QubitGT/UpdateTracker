using System;

namespace Meta.WitAi.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class ButtonAttribute : Attribute
{
	public readonly string displayName;

	public readonly string label;

	public readonly string tooltip;

	public readonly bool isRuntimeOnly;

	public ButtonAttribute(string displayName = null, string label = null, string tooltip = null, bool isRuntimeOnly = false)
	{
		this.displayName = displayName;
		this.label = label;
		this.tooltip = tooltip;
		this.isRuntimeOnly = isRuntimeOnly;
	}
}
