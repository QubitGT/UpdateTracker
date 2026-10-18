using System;
using System.Collections.Generic;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Operations;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tables;

public class AssetTable : DetailedLocalizationTable<AssetTableEntry>, IPreloadRequired
{
	private AsyncOperationHandle m_PreloadOperationHandle;

	private ResourceManager ResourceManager => AddressablesInterface.ResourceManager;

	public virtual AsyncOperationHandle PreloadOperation
	{
		get
		{
			if (!m_PreloadOperationHandle.IsValid())
			{
				m_PreloadOperationHandle = PreloadAssets();
			}
			return m_PreloadOperationHandle;
		}
	}

	private AsyncOperationHandle PreloadAssets()
	{
		PreloadAssetTableMetadata obj = GetMetadata<PreloadAssetTableMetadata>() ?? base.SharedData.Metadata.GetMetadata<PreloadAssetTableMetadata>();
		if (obj == null || obj.Behaviour != 0)
		{
			List<AsyncOperationHandle> list = CollectionPool<List<AsyncOperationHandle>, AsyncOperationHandle>.Get();
			foreach (AssetTableEntry value in base.Values)
			{
				if (!value.IsEmpty && !value.PreloadAsyncOperation.IsValid())
				{
					value.PreloadAsyncOperation = AddressablesInterface.LoadAssetFromGUID<Object[]>(value.Guid);
					list.Add(value.PreloadAsyncOperation);
				}
			}
			if (list.Count > 0)
			{
				return AddressablesInterface.CreateGroupOperation(list);
			}
			CollectionPool<List<AsyncOperationHandle>, AsyncOperationHandle>.Release(list);
		}
		return ResourceManager.CreateCompletedOperation(this, null);
	}

	public AsyncOperationHandle<TObject> GetAssetAsync<TObject>(TableEntryReference entryReference) where TObject : Object
	{
		AssetTableEntry entryFromReference = GetEntryFromReference(entryReference);
		if (entryFromReference == null)
		{
			string text = entryReference.ResolveKeyName(base.SharedData);
			return ResourceManager.CreateCompletedOperation<TObject>(null, "Could not find asset with key \"" + text + "\"");
		}
		return GetAssetAsync<TObject>(entryFromReference);
	}

	internal AsyncOperationHandle<TObject> GetAssetAsync<TObject>(AssetTableEntry entry) where TObject : Object
	{
		if (entry.AsyncOperation.IsValid())
		{
			try
			{
				return entry.AsyncOperation.Convert<TObject>();
			}
			catch (InvalidCastException)
			{
				AddressablesInterface.Release(entry.AsyncOperation);
				entry.AsyncOperation = default(AsyncOperationHandle);
			}
		}
		if (entry.IsEmpty)
		{
			AsyncOperationHandle<TObject> asyncOperationHandle = ResourceManager.CreateCompletedOperation<TObject>(null, null);
			entry.AsyncOperation = asyncOperationHandle;
			return asyncOperationHandle;
		}
		LoadSubAssetOperation<TObject> loadSubAssetOperation = LoadSubAssetOperation<TObject>.Pool.Get();
		loadSubAssetOperation.Init(entry.PreloadAsyncOperation, entry.Address, entry.IsSubAsset, entry.SubAssetName);
		AsyncOperationHandle<TObject> asyncOperationHandle2 = ResourceManager.StartOperation(loadSubAssetOperation, entry.PreloadAsyncOperation);
		entry.AsyncOperation = asyncOperationHandle2;
		return asyncOperationHandle2;
	}

	public void ReleaseAssets()
	{
		if (m_PreloadOperationHandle.IsValid())
		{
			AddressablesInterface.Release(m_PreloadOperationHandle);
			m_PreloadOperationHandle = default(AsyncOperationHandle);
		}
		foreach (AssetTableEntry value in base.Values)
		{
			ReleaseAsset(value);
		}
	}

	public void ReleaseAsset(AssetTableEntry entry)
	{
		if (entry != null)
		{
			if (entry.PreloadAsyncOperation.IsValid())
			{
				AddressablesInterface.Release(entry.PreloadAsyncOperation);
				entry.PreloadAsyncOperation = default(AsyncOperationHandle<Object[]>);
			}
			if (entry.AsyncOperation.IsValid())
			{
				AddressablesInterface.Release(entry.AsyncOperation);
				entry.AsyncOperation = default(AsyncOperationHandle);
			}
		}
	}

	public void ReleaseAsset(TableEntryReference entry)
	{
		ReleaseAsset(GetEntryFromReference(entry));
	}

	public override AssetTableEntry CreateTableEntry()
	{
		return new AssetTableEntry
		{
			Table = this,
			Data = new TableEntryData()
		};
	}
}
