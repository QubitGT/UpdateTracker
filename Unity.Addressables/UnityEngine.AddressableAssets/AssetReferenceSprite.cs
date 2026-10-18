using System;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceSprite : AssetReferenceT<Sprite>
{
	public AssetReferenceSprite(string guid)
		: base(guid)
	{
	}

	public override bool ValidateAsset(string path)
	{
		return false;
	}
}
