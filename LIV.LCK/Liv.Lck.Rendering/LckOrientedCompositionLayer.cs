using UnityEngine;

namespace Liv.Lck.Rendering;

public class LckOrientedCompositionLayer : LckCompositionLayer, ILckOrientationAwareLayer
{
	[Header("Orientation Textures")]
	public Texture HorizontalTexture;

	public Texture VerticalTexture;

	private bool _isHorizontal = true;

	public override Texture CurrentTexture
	{
		get
		{
			if (!_isHorizontal)
			{
				return VerticalTexture;
			}
			return HorizontalTexture;
		}
	}

	public void SetOrientation(bool isHorizontal)
	{
		Debug.Log("LCK LckOrientedCompositionLayer:SetOrientation");
		_isHorizontal = isHorizontal;
	}
}
