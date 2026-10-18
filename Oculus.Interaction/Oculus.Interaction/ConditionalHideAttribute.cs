using UnityEngine;

namespace Oculus.Interaction;

public class ConditionalHideAttribute : PropertyAttribute
{
	public enum DisplayMode
	{
		Always,
		Never,
		ShowIfTrue,
		HideIfTrue
	}

	public string ConditionalFieldPath { get; private set; }

	public object Value { get; private set; }

	public DisplayMode Display { get; private set; } = DisplayMode.ShowIfTrue;


	public ConditionalHideAttribute(string fieldName, object value)
	{
		ConditionalFieldPath = fieldName;
		Value = value;
		Display = DisplayMode.ShowIfTrue;
	}

	public ConditionalHideAttribute(string fieldName, object value, DisplayMode displayMode)
	{
		ConditionalFieldPath = fieldName;
		Value = value;
		Display = displayMode;
	}
}
