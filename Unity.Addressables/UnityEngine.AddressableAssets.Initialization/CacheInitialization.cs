using System;
using System.IO;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.AddressableAssets.Initialization;

[Serializable]
public class CacheInitialization : IInitializableObject
{
	private class CacheInitOp : AsyncOperationBase<bool>, IUpdateReceiver
	{
		private Func<bool> m_Callback;

		private bool m_UpdateRequired = true;

		public void Init(Func<bool> callback)
		{
			m_Callback = callback;
		}

		protected override bool InvokeWaitForCompletion()
		{
			m_RM?.Update(Time.unscaledDeltaTime);
			if (!base.IsDone)
			{
				InvokeExecute();
			}
			return base.IsDone;
		}

		public void Update(float unscaledDeltaTime)
		{
			if (Caching.ready && m_UpdateRequired)
			{
				m_UpdateRequired = false;
				if (m_Callback != null)
				{
					Complete(m_Callback(), success: true, "");
				}
				else
				{
					Complete(result: true, success: true, "");
				}
			}
		}

		protected override void Execute()
		{
			((IUpdateReceiver)this).Update(0f);
		}
	}

	public static string RootPath => Path.GetDirectoryName(Caching.defaultCache.path);

	public bool Initialize(string id, string dataStr)
	{
		CacheInitializationData cacheInitializationData = JsonUtility.FromJson<CacheInitializationData>(dataStr);
		if (cacheInitializationData != null)
		{
			Caching.compressionEnabled = cacheInitializationData.CompressionEnabled;
			Cache currentCacheForWriting = Caching.currentCacheForWriting;
			if (!string.IsNullOrEmpty(cacheInitializationData.CacheDirectoryOverride))
			{
				string text = Addressables.ResolveInternalId(cacheInitializationData.CacheDirectoryOverride);
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				currentCacheForWriting = Caching.GetCacheByPath(text);
				if (!currentCacheForWriting.valid)
				{
					currentCacheForWriting = Caching.AddCache(text);
				}
				Caching.currentCacheForWriting = currentCacheForWriting;
			}
			if (cacheInitializationData.LimitCacheSize)
			{
				currentCacheForWriting.maximumAvailableStorageSpace = cacheInitializationData.MaximumCacheSize;
			}
			else
			{
				currentCacheForWriting.maximumAvailableStorageSpace = long.MaxValue;
			}
			currentCacheForWriting.expirationDelay = 12960000;
		}
		return true;
	}

	public virtual AsyncOperationHandle<bool> InitializeAsync(ResourceManager rm, string id, string data)
	{
		CacheInitOp cacheInitOp = new CacheInitOp();
		cacheInitOp.Init(() => Initialize(id, data));
		return rm.StartOperation(cacheInitOp, default(AsyncOperationHandle));
	}
}
