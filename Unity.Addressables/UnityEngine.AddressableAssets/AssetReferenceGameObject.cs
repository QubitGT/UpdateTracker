using System;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceGameObject : AssetReferenceT<GameObject>
{
	public AssetReferenceGameObject(string guid)
		: base(guid)
	{
	}
}
