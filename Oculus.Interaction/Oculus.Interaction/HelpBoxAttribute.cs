using UnityEngine;

namespace Oculus.Interaction;

public class HelpBoxAttribute : PropertyAttribute
{
	public enum MessageType
	{
		None,
		Info,
		Warning,
		Error
	}

	public delegate bool HelpBoxCondition();

	public string Message { get; private set; }

	public object Value { get; private set; }

	public MessageType Type { get; private set; }

	public ConditionalHideAttribute.DisplayMode Display { get; private set; }

	public HelpBoxAttribute(string message)
	{
		Message = message;
		Type = MessageType.Info;
		Value = null;
		Display = ConditionalHideAttribute.DisplayMode.Always;
	}

	public HelpBoxAttribute(string message, MessageType type)
	{
		Message = message;
		Type = type;
		Value = null;
		Display = ConditionalHideAttribute.DisplayMode.Always;
	}

	public HelpBoxAttribute(string message, MessageType type, object value)
	{
		Message = message;
		Type = type;
		Value = value;
		Display = ConditionalHideAttribute.DisplayMode.ShowIfTrue;
	}

	public HelpBoxAttribute(string message, MessageType type, object value, ConditionalHideAttribute.DisplayMode display)
	{
		Message = message;
		Type = type;
		Value = value;
		Display = display;
	}
}
