using System;
using UnityEngine.U2D;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceAtlasedSprite : AssetReferenceT<Sprite>
{
	public AssetReferenceAtlasedSprite(string guid)
		: base(guid)
	{
	}

	public override bool ValidateAsset(Object obj)
	{
		return obj is SpriteAtlas;
	}

	public override bool ValidateAsset(string path)
	{
		return false;
	}
}
