using System;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceTexture : AssetReferenceT<Texture>
{
	public AssetReferenceTexture(string guid)
		: base(guid)
	{
	}
}
