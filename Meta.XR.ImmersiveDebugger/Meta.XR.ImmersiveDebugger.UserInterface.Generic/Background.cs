using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Background : Controller
{
	private UnityEngine.UI.Image _image;

	public Sprite Sprite
	{
		set
		{
			_image.sprite = value;
		}
	}

	public Color Color
	{
		set
		{
			_image.color = value;
		}
	}

	public float PixelDensityMultiplier
	{
		set
		{
			_image.pixelsPerUnitMultiplier = value;
		}
	}

	public bool RaycastTarget
	{
		set
		{
			_image.raycastTarget = value;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_image = base.GameObject.AddComponent<UnityEngine.UI.Image>();
		_image.type = UnityEngine.UI.Image.Type.Sliced;
		RaycastTarget = false;
	}
}
