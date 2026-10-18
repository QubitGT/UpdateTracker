using System;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceTexture3D : AssetReferenceT<Texture3D>
{
	public AssetReferenceTexture3D(string guid)
		: base(guid)
	{
	}
}
