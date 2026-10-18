using System;
using UnityEngine;

namespace Fusion;

public static class FusionScalableIMGUI
{
	private static GUISkin _scalableSkin;

	private static void InitializedGUIStyles(GUISkin baseSkin)
	{
		_scalableSkin = ((baseSkin == null) ? GUI.skin : baseSkin);
		if (baseSkin == null)
		{
			_scalableSkin = GUI.skin;
			_scalableSkin.button.alignment = TextAnchor.MiddleCenter;
			_scalableSkin.label.alignment = TextAnchor.MiddleCenter;
			_scalableSkin.textField.alignment = TextAnchor.MiddleCenter;
			_scalableSkin.button.normal.background = _scalableSkin.box.normal.background;
			_scalableSkin.button.hover.background = _scalableSkin.window.normal.background;
			_scalableSkin.button.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
			_scalableSkin.button.hover.textColor = new Color(1f, 1f, 1f);
			_scalableSkin.button.active.textColor = new Color(1f, 1f, 1f);
			_scalableSkin.button.border = new RectOffset(6, 6, 6, 6);
			_scalableSkin.window.border = new RectOffset(8, 8, 8, 10);
		}
		else
		{
			_scalableSkin = baseSkin;
		}
	}

	public static GUISkin GetScaledSkin(GUISkin baseSkin, out float height, out float width, out int padding, out int margin, out float boxLeft)
	{
		if (_scalableSkin == null)
		{
			InitializedGUIStyles(baseSkin);
		}
		(height, width, padding, margin, boxLeft) = ScaleGuiSkinToScreenHeight();
		return _scalableSkin;
	}

	public static (float, float, int, int, float) ScaleGuiSkinToScreenHeight()
	{
		_ = Screen.height;
		_ = Screen.width;
		bool num = (float)(Screen.height / Screen.width) > 1.8888888f;
		float num2 = (float)Screen.height * 0.08f;
		float num3 = Math.Min((float)Screen.width * 0.9f, (float)Screen.height * 0.6f);
		int num4 = (int)(num2 / 4f);
		int num5 = (int)(num2 / 8f);
		float item = ((float)Screen.width - num3) * 0.5f;
		int fontSize = (int)(num ? ((num3 - (float)(num4 * 2)) * 0.07f) : (num2 * 0.4f));
		RectOffset margin = new RectOffset(0, 0, num5, num5);
		_scalableSkin.button.fontSize = fontSize;
		_scalableSkin.button.margin = margin;
		_scalableSkin.label.fontSize = fontSize;
		_scalableSkin.label.padding = new RectOffset(num4, num4, num4, num4);
		_scalableSkin.textField.fontSize = fontSize;
		_scalableSkin.window.padding = new RectOffset(num4, num4, num4, num4);
		_scalableSkin.window.margin = new RectOffset(num5, num5, num5, num5);
		return (num2, num3, num4, num5, item);
	}
}
