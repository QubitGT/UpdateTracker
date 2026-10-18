using Meta.XR.ImmersiveDebugger.Manager;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Image : Icon
{
	private WatchTexture _watchTexture;

	private float _defaultHeight;

	public override Texture2D Texture
	{
		internal get
		{
			return base.Texture;
		}
		set
		{
			base.Texture = value;
			if (value != null)
			{
				UpdateSize();
			}
			RefreshLayout();
		}
	}

	internal void Setup(WatchTexture watchTexture)
	{
		_watchTexture = watchTexture;
		_defaultHeight = base.LayoutStyle.size.y;
		Texture = watchTexture.Texture;
	}

	private void UpdateSize()
	{
		int width = Texture.width;
		int height = Texture.height;
		int num = width / height;
		float num2 = Mathf.Min(height, _defaultHeight);
		float x = num2 * (float)num;
		base.LayoutStyle.size = new Vector2(x, num2);
		base.Owner.LayoutStyle.size.y = num2;
	}

	private void Update()
	{
		WatchTexture watchTexture = _watchTexture;
		if (watchTexture != null && watchTexture.Valid && !(_watchTexture.Texture == null))
		{
			Texture = _watchTexture.Texture;
		}
	}
}
