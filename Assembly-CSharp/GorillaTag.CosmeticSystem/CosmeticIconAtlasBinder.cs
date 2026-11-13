using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace GorillaTag.CosmeticSystem;

public static class CosmeticIconAtlasBinder
{
	private const string k_atlasName = "SpriteAtlas";

	private const string k_atlasAddress = "Assets/GorillaTag/Seasonal/SpriteAtlas.spriteatlas";

	private static AsyncOperationHandle<SpriteAtlas> s_loadHandle;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Register()
	{
		s_loadHandle = default(AsyncOperationHandle<SpriteAtlas>);
		SpriteAtlasManager.atlasRequested -= OnAtlasRequested;
		SpriteAtlasManager.atlasRequested += OnAtlasRequested;
	}

	private static void OnAtlasRequested(string tag, Action<SpriteAtlas> bindAtlas)
	{
		if (tag != "SpriteAtlas")
		{
			return;
		}
		if (!s_loadHandle.IsValid())
		{
			s_loadHandle = Addressables.LoadAssetAsync<SpriteAtlas>("Assets/GorillaTag/Seasonal/SpriteAtlas.spriteatlas");
		}
		s_loadHandle.Completed += delegate(AsyncOperationHandle<SpriteAtlas> handle)
		{
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				bindAtlas(handle.Result);
			}
			else
			{
				Debug.LogError("CosmeticIconAtlasBinder: failed to load sprite atlas \"Assets/GorillaTag/Seasonal/SpriteAtlas.spriteatlas\", so " + $"cosmetic icons will be blank. {handle.OperationException}");
			}
		};
	}
}
