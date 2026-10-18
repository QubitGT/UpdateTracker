using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace NexusSDK;

public static class ReferralsAPI
{
	public struct Referral
	{
		public string id { get; set; }

		public string code { get; set; }

		public string playerId { get; set; }

		public string playerName { get; set; }

		public DateTime referralDate { get; set; }
	}

	public struct ReferralError
	{
		public string code { get; set; }

		public string message { get; set; }
	}

	public struct ReferralCodeResponse
	{
		public string code { get; set; }

		public bool isPrimary { get; set; }

		public bool isGenerated { get; set; }

		public bool isManaged { get; set; }
	}

	public delegate void ErrorDelegate(long ErrorCode);

	public struct GetReferralInfoByPlayerIdRequestParams
	{
		public string playerId { get; set; }

		public string groupId { get; set; }

		public int page { get; set; }

		public int pageSize { get; set; }

		public bool excludeReferralList { get; set; }
	}

	public struct GetReferralInfoByPlayerId200Response
	{
		public string groupId { get; set; }

		public string groupName { get; set; }

		public ReferralCodeResponse[] codes { get; set; }

		public string playerId { get; set; }

		public string memberId { get; set; }

		public int currentPage { get; set; }

		public int currentPageSize { get; set; }

		public int totalCount { get; set; }

		public Referral[] referrals { get; set; }
	}

	public delegate void OnGetReferralInfoByPlayerId200ResponseDelegate(GetReferralInfoByPlayerId200Response Param0);

	public delegate void OnGetReferralInfoByPlayerId400ResponseDelegate(ReferralError Param0);

	public struct GetReferralInfoByPlayerIdResponseCallbacks
	{
		public OnGetReferralInfoByPlayerId200ResponseDelegate OnGetReferralInfoByPlayerId200Response { get; set; }

		public OnGetReferralInfoByPlayerId400ResponseDelegate OnGetReferralInfoByPlayerId400Response { get; set; }
	}

	public struct GetPlayerCurrentReferralRequestParams
	{
		public string playerId { get; set; }

		public string groupId { get; set; }
	}

	public delegate void OnGetPlayerCurrentReferral200ResponseDelegate(string Param0);

	public struct GetPlayerCurrentReferral404Response
	{
		public string code { get; set; }
	}

	public delegate void OnGetPlayerCurrentReferral404ResponseDelegate(GetPlayerCurrentReferral404Response Param0);

	public struct GetPlayerCurrentReferralResponseCallbacks
	{
		public OnGetPlayerCurrentReferral200ResponseDelegate OnGetPlayerCurrentReferral200Response { get; set; }

		public OnGetPlayerCurrentReferral404ResponseDelegate OnGetPlayerCurrentReferral404Response { get; set; }
	}

	public struct GetReferralInfoByCodeRequestParams
	{
		public string code { get; set; }

		public string groupId { get; set; }

		public int page { get; set; }

		public int pageSize { get; set; }

		public bool excludeReferralList { get; set; }
	}

	public struct GetReferralInfoByCode200Response
	{
		public string groupId { get; set; }

		public string groupName { get; set; }

		public ReferralCodeResponse[] referralCodes { get; set; }

		public string playerId { get; set; }

		public int currentPage { get; set; }

		public int currentPageSize { get; set; }

		public int totalCount { get; set; }

		public Referral[] referrals { get; set; }
	}

	public delegate void OnGetReferralInfoByCode200ResponseDelegate(GetReferralInfoByCode200Response Param0);

	public delegate void OnGetReferralInfoByCode400ResponseDelegate(ReferralError Param0);

	public struct GetReferralInfoByCodeResponseCallbacks
	{
		public OnGetReferralInfoByCode200ResponseDelegate OnGetReferralInfoByCode200Response { get; set; }

		public OnGetReferralInfoByCode400ResponseDelegate OnGetReferralInfoByCode400Response { get; set; }
	}

	public static IEnumerator StartGetReferralInfoByPlayerIdRequest(GetReferralInfoByPlayerIdRequestParams RequestParams, GetReferralInfoByPlayerIdResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/player/{playerId}";
		text = text.Replace("{playerId}", RequestParams.playerId);
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		list.Add("excludeReferralList=" + RequestParams.excludeReferralList);
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetReferralInfoByPlayerId200Response != null)
			{
				GetReferralInfoByPlayerId200Response param = JsonConvert.DeserializeObject<GetReferralInfoByPlayerId200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetReferralInfoByPlayerId200Response(param);
			}
			break;
		case 400L:
			if (ResponseCallback.OnGetReferralInfoByPlayerId400Response != null)
			{
				ReferralError param2 = JsonConvert.DeserializeObject<ReferralError>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetReferralInfoByPlayerId400Response(param2);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}

	public static IEnumerator StartGetPlayerCurrentReferralRequest(GetPlayerCurrentReferralRequestParams RequestParams, GetPlayerCurrentReferralResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		string text = SDKInitializer.ApiBaseUrl + "/player/{playerId}/code";
		text = text.Replace("{playerId}", RequestParams.playerId);
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetPlayerCurrentReferral200Response != null)
			{
				string text2 = webRequest.downloadHandler.text;
				ResponseCallback.OnGetPlayerCurrentReferral200Response(text2);
			}
			break;
		case 404L:
			if (ResponseCallback.OnGetPlayerCurrentReferral404Response != null)
			{
				GetPlayerCurrentReferral404Response param = JsonConvert.DeserializeObject<GetPlayerCurrentReferral404Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetPlayerCurrentReferral404Response(param);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}

	public static IEnumerator StartGetReferralInfoByCodeRequest(GetReferralInfoByCodeRequestParams RequestParams, GetReferralInfoByCodeResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/code/{code}";
		text = text.Replace("{code}", RequestParams.code);
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		list.Add("excludeReferralList=" + RequestParams.excludeReferralList);
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetReferralInfoByCode200Response != null)
			{
				GetReferralInfoByCode200Response param = JsonConvert.DeserializeObject<GetReferralInfoByCode200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetReferralInfoByCode200Response(param);
			}
			break;
		case 400L:
			if (ResponseCallback.OnGetReferralInfoByCode400Response != null)
			{
				ReferralError param2 = JsonConvert.DeserializeObject<ReferralError>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetReferralInfoByCode400Response(param2);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}
}
