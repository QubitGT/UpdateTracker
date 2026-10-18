using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class ImageStyle : Style
{
	public bool enabled = true;

	public Texture2D icon;

	public Sprite sprite;

	public Color color = Color.white;

	public Color colorHover = Color.white;

	public Color colorOff = Color.white;

	public float pixelDensityMultiplier = 1f;
}
