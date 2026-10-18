using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[IncludeMyAttributes]
[ShowInInspector]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public class ButtonGroupAttribute : PropertyGroupAttribute
{
	[ButtonHeightSelector]
	public int ButtonHeight;

	private IconAlignment buttonIconAlignment;

	private int buttonAlignment;

	private bool stretch;

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "buttonIconAlignment", "HasDefinedButtonIconAlignment" })]
	public IconAlignment IconAlignment
	{
		get
		{
			return buttonIconAlignment;
		}
		set
		{
			buttonIconAlignment = value;
			HasDefinedButtonIconAlignment = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "buttonAlignment", "HasDefinedButtonAlignment" })]
	public int ButtonAlignment
	{
		get
		{
			return buttonAlignment;
		}
		set
		{
			buttonAlignment = value;
			HasDefinedButtonAlignment = true;
		}
	}

	[ShowInInspector]
	[OdinDesignerBinding(new string[] { "stretch", "HasDefinedStretch" })]
	public bool Stretch
	{
		get
		{
			return stretch;
		}
		set
		{
			stretch = value;
			HasDefinedStretch = true;
		}
	}

	public bool HasDefinedButtonIconAlignment { get; private set; }

	public bool HasDefinedButtonAlignment { get; private set; }

	public bool HasDefinedStretch { get; private set; }

	public ButtonGroupAttribute(string group = "_DefaultGroup", float order = 0f)
		: base(group, order)
	{
	}
}
