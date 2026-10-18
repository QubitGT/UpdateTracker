using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Label : Controller
{
	private TextStyle _textStyle;

	internal Text Text { get; private set; }

	public string Content
	{
		get
		{
			return Text.text;
		}
		set
		{
			Text.text = value;
		}
	}

	public TextStyle TextStyle
	{
		get
		{
			return _textStyle;
		}
		set
		{
			_textStyle = value;
			Text.font = value.font;
			Text.fontSize = value.fontSize;
			Text.alignment = value.textAlignement;
			Text.color = value.color;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		Text = base.GameObject.AddComponent<Text>();
		Text.horizontalOverflow = HorizontalWrapMode.Overflow;
		Text.verticalOverflow = VerticalWrapMode.Overflow;
		Text.text = "";
		Text.raycastTarget = false;
	}
}
