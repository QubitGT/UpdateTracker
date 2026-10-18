using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Icon : Controller
{
	private RawImage _image;

	internal RawImage RawImage => _image;

	public virtual Texture2D Texture
	{
		internal get
		{
			return (Texture2D)_image.texture;
		}
		set
		{
			_image.texture = value;
		}
	}

	public Color Color
	{
		set
		{
			_image.color = value;
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
		_image = base.GameObject.AddComponent<RawImage>();
		RaycastTarget = false;
	}
}
