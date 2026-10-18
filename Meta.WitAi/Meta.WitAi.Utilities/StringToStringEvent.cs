using Meta.WitAi.Attributes;
using UnityEngine;

namespace Meta.WitAi.Utilities;

[AddComponentMenu("Wit.ai/Utilities/Conversions/String to String")]
public class StringToStringEvent : MonoBehaviour
{
	[Tooltip("The string format string that will be used to reformat input strings. Ex: I don't know how to respond to {0}")]
	[SerializeField]
	private string _format;

	[Space(8f)]
	[TooltipBox("Triggered when FormatString(float) is called. The string in this event will be formatted based on the format field.")]
	[SerializeField]
	public StringEvent onStringEvent = new StringEvent();

	public void FormatString(string format, string value)
	{
		if (string.IsNullOrEmpty(format))
		{
			onStringEvent?.Invoke(value);
		}
		else
		{
			onStringEvent?.Invoke(string.Format(format, value));
		}
	}

	public void FormatString(string value)
	{
		FormatString(_format, value);
	}
}
