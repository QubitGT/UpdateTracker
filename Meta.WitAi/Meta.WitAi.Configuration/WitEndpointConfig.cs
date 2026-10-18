using System;
using Meta.WitAi.Data.Configuration;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Configuration;

[Serializable]
public class WitEndpointConfig : IWitRequestEndpointInfo
{
	[SerializeField]
	[FormerlySerializedAs("uriScheme")]
	private string _uriScheme;

	[SerializeField]
	[FormerlySerializedAs("authority")]
	private string _authority;

	[SerializeField]
	[FormerlySerializedAs("port")]
	private int _port;

	[SerializeField]
	[FormerlySerializedAs("witApiVersion")]
	private string _witApiVersion;

	[SerializeField]
	[FormerlySerializedAs("message")]
	private string _message;

	[SerializeField]
	[FormerlySerializedAs("speech")]
	private string _speech;

	[SerializeField]
	[FormerlySerializedAs("dictation")]
	private string _dictation;

	[SerializeField]
	private string _synthesize;

	[SerializeField]
	private string _event;

	[SerializeField]
	private string _converse;

	private static WitEndpointConfig defaultEndpointConfig = new WitEndpointConfig();

	public string UriScheme
	{
		get
		{
			if (!string.IsNullOrEmpty(_uriScheme))
			{
				return _uriScheme;
			}
			return "https";
		}
	}

	public string Authority
	{
		get
		{
			if (!string.IsNullOrEmpty(_authority))
			{
				return _authority;
			}
			return "api.wit.ai";
		}
	}

	public int Port
	{
		get
		{
			if (_port > 0)
			{
				return _port;
			}
			return -1;
		}
	}

	public string WitApiVersion
	{
		get
		{
			if (!string.IsNullOrEmpty(_witApiVersion))
			{
				return _witApiVersion;
			}
			return "20250213";
		}
	}

	public string Message
	{
		get
		{
			if (!string.IsNullOrEmpty(_message))
			{
				return _message;
			}
			return "message";
		}
	}

	public string Speech
	{
		get
		{
			if (!string.IsNullOrEmpty(_speech))
			{
				return _speech;
			}
			return "speech";
		}
	}

	public string Dictation
	{
		get
		{
			if (!string.IsNullOrEmpty(_dictation))
			{
				return _dictation;
			}
			return "dictation";
		}
	}

	public string Synthesize
	{
		get
		{
			if (!string.IsNullOrEmpty(_synthesize))
			{
				return _synthesize;
			}
			return "synthesize";
		}
	}

	public string Event
	{
		get
		{
			if (!string.IsNullOrEmpty(_event))
			{
				return _event;
			}
			return "event";
		}
	}

	public string Converse
	{
		get
		{
			if (!string.IsNullOrEmpty(_converse))
			{
				return _converse;
			}
			return "converse";
		}
	}

	public static WitEndpointConfig GetEndpointConfig(WitConfiguration witConfig)
	{
		if (!witConfig || witConfig.endpointConfiguration == null)
		{
			return defaultEndpointConfig;
		}
		return witConfig.endpointConfiguration;
	}
}
