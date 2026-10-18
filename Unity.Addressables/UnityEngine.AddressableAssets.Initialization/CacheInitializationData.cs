using System;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets.Initialization;

[Serializable]
public class CacheInitializationData
{
	[FormerlySerializedAs("m_compressionEnabled")]
	[SerializeField]
	private bool m_CompressionEnabled = true;

	[FormerlySerializedAs("m_cacheDirectoryOverride")]
	[SerializeField]
	private string m_CacheDirectoryOverride = "";

	[FormerlySerializedAs("m_limitCacheSize")]
	[SerializeField]
	private bool m_LimitCacheSize;

	[FormerlySerializedAs("m_maximumCacheSize")]
	[SerializeField]
	private long m_MaximumCacheSize = long.MaxValue;

	public bool CompressionEnabled
	{
		get
		{
			return m_CompressionEnabled;
		}
		set
		{
			m_CompressionEnabled = value;
		}
	}

	public string CacheDirectoryOverride
	{
		get
		{
			return m_CacheDirectoryOverride;
		}
		set
		{
			m_CacheDirectoryOverride = value;
		}
	}

	public bool LimitCacheSize
	{
		get
		{
			return m_LimitCacheSize;
		}
		set
		{
			m_LimitCacheSize = value;
		}
	}

	public long MaximumCacheSize
	{
		get
		{
			return m_MaximumCacheSize;
		}
		set
		{
			m_MaximumCacheSize = value;
		}
	}
}
