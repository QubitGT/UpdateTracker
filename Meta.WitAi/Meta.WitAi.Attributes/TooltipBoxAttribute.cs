using UnityEngine;

namespace Meta.WitAi.Attributes;

public class TooltipBoxAttribute : PropertyAttribute
{
	public string Text { get; private set; }

	public TooltipBoxAttribute(string text)
	{
		Text = text;
	}
}
