using Meta.WitAi.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Utilities;

[AddComponentMenu("Wit.ai/Utilities/Conversions/Float to String")]
public class FloatToStringEvent : MonoBehaviour
{
	[FormerlySerializedAs("format")]
	[Tooltip("The format value to be used on the float")]
	[SerializeField]
	private string _floatFormat;

	[Tooltip("The format of the string itself. {0} will represent the float value provided")]
	[SerializeField]
	private string _stringFormat;

	[Space(8f)]
	[TooltipBox("Triggered when ConvertFloatToString(float) is called. The string in this event will be formatted based on the format fields.")]
	[SerializeField]
	private StringEvent onFloatToString = new StringEvent();

	public void ConvertFloatToString(float value)
	{
		string arg = ((!string.IsNullOrEmpty(_floatFormat)) ? value.ToString(_floatFormat) : value.ToString());
		if (string.IsNullOrEmpty(_stringFormat))
		{
			onFloatToString?.Invoke(arg);
		}
		else
		{
			onFloatToString?.Invoke(string.Format(_stringFormat, arg));
		}
	}
}
