using System;
using Liv.Lck.Rendering;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

[Serializable]
public class LckOverlayFrameLayer : LckOrientedCompositionLayer
{
	public override Texture CurrentTexture
	{
		get
		{
			if (IsActive)
			{
				return base.CurrentTexture;
			}
			return null;
		}
	}

	public LckOverlayFrameLayer()
	{
		Name = "Overlay Frame";
	}
}
