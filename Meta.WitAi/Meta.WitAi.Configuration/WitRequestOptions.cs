using System;
using System.Collections.Generic;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.WitAi.Configuration;

public class WitRequestOptions : VoiceServiceRequestOptions
{
	public IDynamicEntitiesProvider dynamicEntities;

	public int nBestIntents = -1;

	[Obsolete("Use WitConfiguration.editorVersionTag or WitConfiguration.buildVersionTag")]
	[SerializeField]
	[HideInInspector]
	public string tag;

	public Action<WitRequest> onResponse;

	[Obsolete("Use 'RequestId' property instead")]
	[JsonIgnore]
	public string requestID => base.RequestId;

	public static Dictionary<string, string> OpIdRegistry { get; set; } = new Dictionary<string, string>();


	public WitRequestOptions(params QueryParam[] newParams)
		: base(newParams)
	{
	}

	public WitRequestOptions(string newRequestId, string newClientUserId, string newOperationId, params QueryParam[] newParams)
		: base(newRequestId, newClientUserId, newOperationId, newParams)
	{
	}

	public string ToJsonString()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["nBestIntents"] = nBestIntents.ToString();
		dictionary["requestID"] = base.RequestId;
		if (!string.IsNullOrEmpty(base.OperationId))
		{
			dictionary["operationId"] = base.OperationId;
			OpIdRegistry[base.RequestId] = base.OperationId;
		}
		foreach (string key in base.QueryParams.Keys)
		{
			dictionary[key] = base.QueryParams[key];
		}
		return JsonConvert.SerializeObject(dictionary);
	}
}
