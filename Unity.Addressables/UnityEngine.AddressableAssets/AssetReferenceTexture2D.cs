using System;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceTexture2D : AssetReferenceT<Texture2D>
{
	public AssetReferenceTexture2D(string guid)
		: base(guid)
	{
	}
}
