using System;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReferenceT<TObject> : AssetReference where TObject : Object
{
	public AssetReferenceT(string guid)
		: base(guid)
	{
	}

	public virtual AsyncOperationHandle<TObject> LoadAssetAsync()
	{
		return LoadAssetAsync<TObject>();
	}

	public override bool ValidateAsset(Object obj)
	{
		Type type = obj.GetType();
		return typeof(TObject).IsAssignableFrom(type);
	}

	public override bool ValidateAsset(string mainAssetPath)
	{
		return false;
	}
}
