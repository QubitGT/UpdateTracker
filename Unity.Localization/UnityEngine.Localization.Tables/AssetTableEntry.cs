using System;
using UnityEngine.Localization.Metadata;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tables;

public class AssetTableEntry : TableEntry
{
	private string m_GuidCache;

	private string m_SubAssetNameCache;

	internal AsyncOperationHandle<Object[]> PreloadAsyncOperation { get; set; }

	internal AsyncOperationHandle AsyncOperation { get; set; }

	public string Address
	{
		get
		{
			return base.Data.Localized;
		}
		set
		{
			base.Data.Localized = value;
			m_GuidCache = null;
			m_SubAssetNameCache = null;
		}
	}

	public string Guid
	{
		get
		{
			if (m_GuidCache == null)
			{
				m_GuidCache = AssetAddress.GetGuid(Address);
			}
			return m_GuidCache;
		}
		set
		{
			Address = value;
		}
	}

	public string SubAssetName
	{
		get
		{
			if (m_SubAssetNameCache == null)
			{
				m_SubAssetNameCache = AssetAddress.GetSubAssetName(Address);
			}
			return m_SubAssetNameCache;
		}
	}

	public bool IsEmpty => string.IsNullOrEmpty(Address);

	public bool IsSubAsset => AssetAddress.IsSubAsset(Address);

	internal AssetTableEntry()
	{
	}

	public void RemoveFromTable()
	{
		AssetTable assetTable = base.Table as AssetTable;
		if (assetTable == null)
		{
			Debug.LogWarning(string.Format("Failed to remove {0} with id {1} and address `{2}` as it does not belong to a table.", "AssetTableEntry", base.KeyId, Address));
		}
		else
		{
			assetTable.Remove(base.KeyId);
		}
	}

	internal Type GetExpectedType()
	{
		foreach (IMetadata metadataEntry in base.Table.SharedData.Metadata.MetadataEntries)
		{
			if (metadataEntry is AssetTypeMetadata assetTypeMetadata && assetTypeMetadata.Contains(base.KeyId))
			{
				return assetTypeMetadata.Type;
			}
		}
		return typeof(Object);
	}

	public void SetAssetOverride<T>(T asset) where T : Object
	{
		AddressablesInterface.SafeRelease(AsyncOperation);
		AsyncOperation = AddressablesInterface.ResourceManager.CreateCompletedOperation(asset, null);
	}
}
