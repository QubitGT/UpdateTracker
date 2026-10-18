using System.Collections.Generic;
using Meta.Voice;

namespace Meta.WitAi.Requests;

public class VoiceServiceRequestOptions : INLPRequestOptions, ITranscriptionRequestOptions, IVoiceRequestOptions
{
	public class QueryParam
	{
		public string key;

		public string value;
	}

	public string RequestId { get; private set; }

	public string ClientUserId { get; private set; }

	public string OperationId { get; set; }

	public int TimeoutMs { get; set; }

	public Dictionary<string, string> QueryParams { get; private set; }

	public NLPRequestInputType InputType { get; set; }

	public string Text { get; set; }

	public float AudioThreshold { get; set; }

	public VoiceServiceRequestOptions(string newRequestId, string newClientUserId, string newOperationId, params QueryParam[] newParams)
	{
		RequestId = (string.IsNullOrEmpty(newRequestId) ? WitConstants.GetUniqueId() : newRequestId);
		ClientUserId = (string.IsNullOrEmpty(newClientUserId) ? WitRequestSettings.LocalClientUserId : newClientUserId);
		OperationId = (string.IsNullOrEmpty(newOperationId) ? WitConstants.GetUniqueId() : newOperationId);
		QueryParams = ConvertQueryParams(newParams);
	}

	public VoiceServiceRequestOptions(params QueryParam[] newParams)
		: this(null, newParams)
	{
	}

	public VoiceServiceRequestOptions(string newRequestId, params QueryParam[] newParams)
		: this(newRequestId, null, null, newParams)
	{
	}

	public static Dictionary<string, string> ConvertQueryParams(QueryParam[] newParams)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (newParams != null)
		{
			foreach (QueryParam queryParam in newParams)
			{
				if (!string.IsNullOrEmpty(queryParam.key))
				{
					dictionary[queryParam.key] = dictionary[queryParam.value];
				}
			}
		}
		return dictionary;
	}

	public void SetOperationId(string opId)
	{
		OperationId = opId;
	}
}
