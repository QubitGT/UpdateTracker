using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace NexusSDK;

public static class AttributionAPI
{
	public struct Code
	{
		public string code { get; set; }

		public bool isPrimary { get; set; }

		public bool isGenerated { get; set; }

		public bool isManaged { get; set; }
	}

	public enum Currency
	{
		AED,
		AFN,
		ALL,
		AMD,
		ANG,
		AOA,
		ARS,
		AUD,
		AWG,
		AZN,
		BAM,
		BBD,
		BDT,
		BGN,
		BHD,
		BIF,
		BMD,
		BND,
		BOB,
		BRL,
		BSD,
		BTC,
		BTN,
		BWP,
		BYN,
		BYR,
		BZD,
		CAD,
		CDF,
		CHF,
		CLF,
		CLP,
		CNY,
		COP,
		CRC,
		CUC,
		CUP,
		CVE,
		CZK,
		DJF,
		DKK,
		DOP,
		DZD,
		EGP,
		ERN,
		ETB,
		EUR,
		FJD,
		FKP,
		GBP,
		GEL,
		GGP,
		GHS,
		GIP,
		GMD,
		GNF,
		GTQ,
		GYD,
		HKD,
		HNL,
		HRK,
		HTG,
		HUF,
		IDR,
		ILS,
		IMP,
		INR,
		IQD,
		IRR,
		ISK,
		JEP,
		JMD,
		JOD,
		JPY,
		KES,
		KGS,
		KHR,
		KMF,
		KPW,
		KRW,
		KWD,
		KYD,
		KZT,
		LAK,
		LBP,
		LKR,
		LRD,
		LSL,
		LTL,
		LVL,
		LYD,
		MAD,
		MDL,
		MGA,
		MKD,
		MMK,
		MNT,
		MOP,
		MRO,
		MUR,
		MVR,
		MWK,
		MXN,
		MYR,
		MZN,
		NAD,
		NGN,
		NIO,
		NOK,
		NPR,
		NZD,
		OMR,
		PAB,
		PEN,
		PGK,
		PHP,
		PKR,
		PLN,
		PYG,
		QAR,
		RON,
		RSD,
		RUB,
		RWF,
		SAR,
		SBD,
		SCR,
		SDG,
		SEK,
		SGD,
		SHP,
		SLL,
		SOS,
		SRD,
		STD,
		SVC,
		SYP,
		SZL,
		THB,
		TJS,
		TMT,
		TND,
		TOP,
		TRY,
		TTD,
		TWD,
		TZS,
		UAH,
		UGX,
		USD,
		UYU,
		UZS,
		VEF,
		VND,
		VUV,
		WST,
		XAF,
		XAG,
		XAU,
		XCD,
		XDR,
		XOF,
		XPF,
		YER,
		ZAR,
		ZMK,
		ZMW,
		ZWL
	}

	public struct Metrics
	{
		public struct conversion_Struct
		{
			public struct totalSpendToDate_Struct
			{
				public double total { get; set; }

				public string currency { get; set; }
			}

			public DateTime lastPurchaseDate { get; set; }

			public totalSpendToDate_Struct totalSpendToDate { get; set; }
		}

		public DateTime joinDate { get; set; }

		public conversion_Struct conversion { get; set; }
	}

	public struct Transaction
	{
		public string id { get; set; }

		public string memberId { get; set; }

		public string code { get; set; }

		public string memberPlayerId { get; set; }

		public string description { get; set; }

		public string status { get; set; }

		public double subtotal { get; set; }

		public string currency { get; set; }

		public double total { get; set; }

		public string totalCurrency { get; set; }

		public string transactionId { get; set; }

		public DateTime transactionDate { get; set; }

		public string platform { get; set; }

		public string playerId { get; set; }

		public string playerName { get; set; }

		public Metrics metrics { get; set; }

		public double memberShareAmount { get; set; }

		public double memberSharePercent { get; set; }

		public bool memberPaid { get; set; }

		public string skuId { get; set; }
	}

	public struct Member
	{
		public string id { get; set; }

		public string name { get; set; }

		public string playerId { get; set; }

		public PlayerMetadata playerMetadata { get; set; }

		public string logoImage { get; set; }

		public string profileImage { get; set; }

		public Code[] codes { get; set; }
	}

	public struct PlayerMetadata
	{
		public string displayName { get; set; }
	}

	public struct CreatorGroup
	{
		public string name { get; set; }

		public string id { get; set; }

		public bool isDefault { get; set; }
	}

	public struct ScheduledRevShare
	{
		public string id { get; set; }

		public double revShare { get; set; }

		public DateTime startDate { get; set; }

		public DateTime endDate { get; set; }

		public string groupId { get; set; }

		public string groupName { get; set; }

		public TierRevenueShare[] tierRevenueShares { get; set; }
	}

	public struct CreatorGroupTier
	{
		public string id { get; set; }

		public string name { get; set; }

		public double revShare { get; set; }

		public double memberCount { get; set; }
	}

	public struct TierRevenueShare
	{
		public double revShare { get; set; }

		public string tierId { get; set; }

		public string tierName { get; set; }
	}

	public struct APIError
	{
		public string code { get; set; }

		public string message { get; set; }
	}

	public delegate void ErrorDelegate(long ErrorCode);

	public struct GetMembersRequestParams
	{
		public int page { get; set; }

		public int pageSize { get; set; }

		public string groupId { get; set; }
	}

	public struct GetMembers200Response
	{
		public string groupId { get; set; }

		public string groupName { get; set; }

		public int currentPage { get; set; }

		public int currentPageSize { get; set; }

		public int totalCount { get; set; }

		public Member[] members { get; set; }
	}

	public delegate void OnGetMembers200ResponseDelegate(GetMembers200Response Param0);

	public struct GetMemberByCodeOrUuidRequestParams
	{
		public string memberCodeOrID { get; set; }

		public string groupId { get; set; }
	}

	public delegate void OnGetMemberByCodeOrUuid200ResponseDelegate(Member Param0);

	public struct GetMemberByPlayerIdRequestParams
	{
		public string playerId { get; set; }

		public string groupId { get; set; }
	}

	public delegate void OnGetMemberByPlayerId200ResponseDelegate(Member Param0);

	public delegate void OnGetAttributionsPing200ResponseDelegate();

	public static IEnumerator StartGetMembersRequest(GetMembersRequestParams RequestParams, OnGetMembers200ResponseDelegate ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/manage/members";
		List<string> list = new List<string>();
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		if (webRequest.responseCode == 200)
		{
			if (ResponseCallback != null)
			{
				GetMembers200Response param = JsonConvert.DeserializeObject<GetMembers200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback?.Invoke(param);
			}
		}
		else
		{
			ErrorCallback?.Invoke(webRequest.responseCode);
		}
	}

	public static IEnumerator StartGetMemberByCodeOrUuidRequest(GetMemberByCodeOrUuidRequestParams RequestParams, OnGetMemberByCodeOrUuid200ResponseDelegate ResponseCallback, ErrorDelegate ErrorCallback)
	{
		string text = SDKInitializer.ApiBaseUrl + "/manage/members/{memberCodeOrID}";
		text = text.Replace("{memberCodeOrID}", RequestParams.memberCodeOrID);
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
		if (webRequest.responseCode == 200)
		{
			if (ResponseCallback != null)
			{
				Member param = JsonConvert.DeserializeObject<Member>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback?.Invoke(param);
			}
		}
		else
		{
			ErrorCallback?.Invoke(webRequest.responseCode);
		}
	}

	public static IEnumerator StartGetMemberByPlayerIdRequest(GetMemberByPlayerIdRequestParams RequestParams, OnGetMemberByPlayerId200ResponseDelegate ResponseCallback, ErrorDelegate ErrorCallback)
	{
		string text = SDKInitializer.ApiBaseUrl + "/manage/members/player/{playerId}";
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
		if (webRequest.responseCode == 200)
		{
			if (ResponseCallback != null)
			{
				Member param = JsonConvert.DeserializeObject<Member>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback?.Invoke(param);
			}
		}
		else
		{
			ErrorCallback?.Invoke(webRequest.responseCode);
		}
	}

	public static IEnumerator StartGetAttributionsPingRequest(OnGetAttributionsPing200ResponseDelegate ResponseCallback, ErrorDelegate ErrorCallback)
	{
		string uri = SDKInitializer.ApiBaseUrl + "/attributions/ping";
		using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		if (webRequest.responseCode == 200)
		{
			if (ResponseCallback != null)
			{
				ResponseCallback?.Invoke();
			}
		}
		else
		{
			ErrorCallback?.Invoke(webRequest.responseCode);
		}
	}
}
