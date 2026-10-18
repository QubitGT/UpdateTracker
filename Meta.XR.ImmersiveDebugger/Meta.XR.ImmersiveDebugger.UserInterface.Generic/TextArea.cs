using System;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class TextArea : Value
{
	private Text Text => base.Label.Text;

	internal override string Content
	{
		get
		{
			return Text.text;
		}
		set
		{
			string text = value.Replace("\\n", Environment.NewLine);
			Text.text = text;
			UpdateLayoutSize();
		}
	}

	internal float TextAreaHeight => CalculateHeight(base.LayoutStyle.size.x);

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		Text.horizontalOverflow = HorizontalWrapMode.Wrap;
		Text.verticalOverflow = VerticalWrapMode.Overflow;
		Text.text = "";
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		Text.color = (base.Transparent ? Color.white : base.TextStyle.color);
	}

	internal void UpdateLayoutSize()
	{
		base.LayoutStyle.size.y = TextAreaHeight + base.Owner.LayoutStyle.spacing + base.Label.LayoutStyle.margin.y * 2f;
		RefreshLayout();
	}

	private float CalculateHeight(float textWidth)
	{
		TextGenerationSettings settings = default(TextGenerationSettings);
		settings.generationExtents = new Vector2(textWidth, 0f);
		settings.fontSize = Text.fontSize;
		settings.textAnchor = Text.alignment;
		settings.alignByGeometry = Text.alignByGeometry;
		settings.scaleFactor = Text.pixelsPerUnit;
		settings.color = Text.color;
		settings.font = Text.font;
		settings.pivot = base.RectTransform.pivot;
		settings.richText = false;
		settings.lineSpacing = Text.lineSpacing;
		settings.fontStyle = Text.fontStyle;
		settings.resizeTextForBestFit = false;
		settings.updateBounds = true;
		settings.horizontalOverflow = Text.horizontalOverflow;
		settings.verticalOverflow = Text.verticalOverflow;
		TextGenerator textGenerator = new TextGenerator();
		textGenerator.Populate(Text.text, settings);
		return textGenerator.rectExtents.height / Text.pixelsPerUnit;
	}
}
