using System;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.UI;

[Feature(Feature.VirtualKeyboard)]
public class OVRVirtualKeyboardInputFieldTextHandler : OVRVirtualKeyboard.AbstractTextHandler
{
	[SerializeField]
	private InputField inputField;

	private bool _isSelected;

	public InputField InputField
	{
		get
		{
			return inputField;
		}
		set
		{
			if (!(value == inputField))
			{
				if ((bool)inputField)
				{
					inputField.onValueChanged.RemoveListener(ProxyOnValueChanged);
				}
				inputField = value;
				if ((bool)inputField)
				{
					inputField.onValueChanged.AddListener(ProxyOnValueChanged);
				}
				OnTextChanged?.Invoke(Text);
			}
		}
	}

	public override Action<string> OnTextChanged { get; set; }

	public override string Text
	{
		get
		{
			if (!inputField)
			{
				return string.Empty;
			}
			return inputField.text;
		}
	}

	public override bool SubmitOnEnter
	{
		get
		{
			if ((bool)inputField)
			{
				return inputField.lineType != InputField.LineType.MultiLineNewline;
			}
			return false;
		}
	}

	public override bool IsFocused
	{
		get
		{
			if ((bool)inputField)
			{
				return inputField.isFocused;
			}
			return false;
		}
	}

	public override void Submit()
	{
		if ((bool)inputField)
		{
			inputField.onEndEdit.Invoke(inputField.text);
		}
	}

	public override void AppendText(string s)
	{
		if ((bool)inputField)
		{
			inputField.text += s;
		}
	}

	public override void ApplyBackspace()
	{
		if ((bool)inputField && !string.IsNullOrEmpty(inputField.text))
		{
			inputField.text = Text.Substring(0, Text.Length - 1);
		}
	}

	public override void MoveTextEnd()
	{
		if ((bool)inputField)
		{
			inputField.MoveTextEnd(shift: false);
		}
	}

	protected void Start()
	{
		if ((bool)inputField)
		{
			inputField.onValueChanged.AddListener(ProxyOnValueChanged);
		}
	}

	protected void ProxyOnValueChanged(string arg0)
	{
		OnTextChanged?.Invoke(arg0);
	}
}
