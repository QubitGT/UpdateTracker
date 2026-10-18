using System;
using System.Collections.Generic;
using System.Linq;
using Meta.Voice.Net.WebSockets;
using Meta.WitAi.Composer.Attributes;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data.Info;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Data.Configuration;

public class WitConfiguration : ScriptableObject, IWitRequestConfiguration, IWitWebSocketClientProvider
{
	[Tooltip("Access token used in builds to make requests for data from Wit.ai")]
	[FormerlySerializedAs("clientAccessToken")]
	[SerializeField]
	private string _clientAccessToken;

	[Tooltip("Which deployed version to use in editor (defaults to current when empty)")]
	[VersionTagDropdown]
	public string editorVersionTag;

	[Tooltip("Which deployed version to use in a build (defaults to current when empty)")]
	[VersionTagDropdown]
	public string buildVersionTag;

	[FormerlySerializedAs("application")]
	[SerializeField]
	private WitAppInfo _appInfo;

	[SerializeField]
	private WitConfigurationAssetData[] _configData;

	[FormerlySerializedAs("configId")]
	[HideInInspector]
	[SerializeField]
	private string _configurationId;

	[Tooltip("The request connection type to be used by all requests made with this configuration.")]
	[SerializeField]
	private WitRequestType _requestType;

	[Tooltip("The number of milliseconds to wait before requests to Wit.ai will timeout")]
	[FormerlySerializedAs("timeoutMS")]
	[SerializeField]
	private int _requestTimeoutMs = 10000;

	[Tooltip("Configuration parameters to set up a custom endpoint for testing purposes and request forwarding. The default values here will work for most.")]
	[SerializeField]
	public WitEndpointConfig endpointConfiguration = new WitEndpointConfig();

	[SerializeField]
	public bool isDemoOnly;

	[Tooltip("Intent attributes (ex: [MatchIntent('change-color')] void ChangeColor(string color) are useful for quickly addressing voice commands in code, but they come at the cost of reflection. If you don't  need these or don't want to pay the reflection cost it is recommended you turn these off.")]
	[SerializeField]
	public bool useIntentAttributes = true;

	[Tooltip("Conduit enables manifest-based dispatching to invoke callbacks with native types directly without requiring manual parsing.")]
	[SerializeField]
	public bool useConduit = true;

	[SerializeField]
	private string _manifestLocalPath;

	[SerializeField]
	public List<string> excludedAssemblies = new List<string> { "Oculus.Voice.Demo, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", "Meta.WitAi.Samples, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" };

	[Tooltip("When true, Conduit will attempt to match incoming requests by type when no exact matches are found. This increases tolerance but reduces runtime performance.")]
	[SerializeField]
	public bool relaxedResolution;

	private const string INVALID_APP_ID_NO_CLIENT_TOKEN = "App Info Not Set - No Client Token";

	private const string INVALID_APP_ID_WITH_CLIENT_TOKEN = "App Info Not Set - Has Client Token";

	private WitWebSocketClient _client;

	public WitRequestType RequestType
	{
		get
		{
			return _requestType;
		}
		set
		{
			_requestType = value;
		}
	}

	public int RequestTimeoutMs
	{
		get
		{
			return _requestTimeoutMs;
		}
		set
		{
			_requestTimeoutMs = value;
		}
	}

	[Obsolete("Deprecated in favor of 'RequestTimeoutMs'. Access will be removed in the future.")]
	public int timeoutMS
	{
		get
		{
			return RequestTimeoutMs;
		}
		set
		{
			RequestTimeoutMs = value;
		}
	}

	public string ManifestLocalPath => _manifestLocalPath;

	public IWitWebSocketClient WebSocketClient
	{
		get
		{
			if (_client == null)
			{
				_client = new WitWebSocketClient(this);
			}
			return _client;
		}
	}

	public string GetVersionTag()
	{
		return buildVersionTag;
	}

	public void ResetData()
	{
		_configurationId = null;
		_appInfo = default(WitAppInfo);
		endpointConfiguration = new WitEndpointConfig();
	}

	public void UpdateDataAssets()
	{
	}

	public string GetLoggerAppId()
	{
		string applicationId = GetApplicationId();
		if (string.IsNullOrEmpty(applicationId))
		{
			if (!string.IsNullOrEmpty(GetClientAccessToken()))
			{
				return "App Info Not Set - Has Client Token";
			}
			return "App Info Not Set - No Client Token";
		}
		return applicationId;
	}

	public string GetConfigurationId()
	{
		return _configurationId;
	}

	public string GetApplicationId()
	{
		return _appInfo.id;
	}

	public WitAppInfo GetApplicationInfo()
	{
		return _appInfo;
	}

	public WitConfigurationAssetData[] GetConfigData()
	{
		if (_configData == null)
		{
			_configData = Array.Empty<WitConfigurationAssetData>();
		}
		return _configData;
	}

	public TConfigData GetConfigData<TConfigData>() where TConfigData : WitConfigurationAssetData
	{
		if (_configData == null)
		{
			return null;
		}
		return _configData.FirstOrDefault((WitConfigurationAssetData data) => data?.GetType() == typeof(TConfigData)) as TConfigData;
	}

	public IWitRequestEndpointInfo GetEndpointInfo()
	{
		return endpointConfiguration;
	}

	public string GetClientAccessToken()
	{
		return _clientAccessToken;
	}

	public void SetApplicationInfo(WitAppInfo newInfo)
	{
		_appInfo = newInfo;
	}

	public void SetClientAccessToken(string newToken)
	{
		_clientAccessToken = newToken;
	}
}
