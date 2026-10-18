using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets.Initialization;

[Serializable]
public class ResourceManagerRuntimeData
{
	public const string kCatalogAddress = "AddressablesMainContentCatalog";

	[SerializeField]
	private string m_buildTarget;

	[FormerlySerializedAs("m_settingsHash")]
	[SerializeField]
	private string m_SettingsHash;

	[FormerlySerializedAs("m_catalogLocations")]
	[SerializeField]
	private List<ResourceLocationData> m_CatalogLocations = new List<ResourceLocationData>();

	[FormerlySerializedAs("m_logResourceManagerExceptions")]
	[SerializeField]
	private bool m_LogResourceManagerExceptions = true;

	[FormerlySerializedAs("m_extraInitializationData")]
	[SerializeField]
	private List<ObjectInitializationData> m_ExtraInitializationData = new List<ObjectInitializationData>();

	[SerializeField]
	private bool m_DisableCatalogUpdateOnStart;

	[SerializeField]
	private bool m_IsLocalCatalogInBundle;

	[SerializeField]
	private SerializedType m_CertificateHandlerType;

	[SerializeField]
	private string m_AddressablesVersion;

	[SerializeField]
	private int m_maxConcurrentWebRequests = 500;

	[SerializeField]
	private int m_CatalogRequestsTimeout;

	public string BuildTarget
	{
		get
		{
			return m_buildTarget;
		}
		set
		{
			m_buildTarget = value;
		}
	}

	public string SettingsHash
	{
		get
		{
			return m_SettingsHash;
		}
		set
		{
			m_SettingsHash = value;
		}
	}

	public List<ResourceLocationData> CatalogLocations => m_CatalogLocations;

	public bool LogResourceManagerExceptions
	{
		get
		{
			return m_LogResourceManagerExceptions;
		}
		set
		{
			m_LogResourceManagerExceptions = value;
		}
	}

	public List<ObjectInitializationData> InitializationObjects => m_ExtraInitializationData;

	public bool DisableCatalogUpdateOnStartup
	{
		get
		{
			return m_DisableCatalogUpdateOnStart;
		}
		set
		{
			m_DisableCatalogUpdateOnStart = value;
		}
	}

	public bool IsLocalCatalogInBundle
	{
		get
		{
			return m_IsLocalCatalogInBundle;
		}
		set
		{
			m_IsLocalCatalogInBundle = value;
		}
	}

	public Type CertificateHandlerType
	{
		get
		{
			return m_CertificateHandlerType.Value;
		}
		set
		{
			m_CertificateHandlerType.Value = value;
		}
	}

	public string AddressablesVersion
	{
		get
		{
			return m_AddressablesVersion;
		}
		set
		{
			m_AddressablesVersion = value;
		}
	}

	public int MaxConcurrentWebRequests
	{
		get
		{
			return m_maxConcurrentWebRequests;
		}
		set
		{
			m_maxConcurrentWebRequests = Mathf.Clamp(value, 1, 1024);
		}
	}

	public int CatalogRequestsTimeout
	{
		get
		{
			return m_CatalogRequestsTimeout;
		}
		set
		{
			m_CatalogRequestsTimeout = ((value >= 0) ? value : 0);
		}
	}
}
