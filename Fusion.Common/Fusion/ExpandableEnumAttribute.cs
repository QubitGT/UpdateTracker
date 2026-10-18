using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public class ExpandableEnumAttribute : DrawerPropertyAttribute
{
	public bool AlwaysExpanded { get; set; } = false;


	public bool ShowFlagsButtons { get; set; } = true;


	public bool ShowInlineHelp { get; set; } = false;

}
