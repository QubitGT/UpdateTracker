using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace NexusSDK;

public static class BountyAPI
{
	public struct Bounty
	{
		public struct dependants_Struct_Element
		{
			public string id { get; set; }

			public string name { get; set; }
		}

		public struct prerequisites_Struct_Element
		{
			public string id { get; set; }

			public string name { get; set; }
		}

		public string id { get; set; }

		public string name { get; set; }

		public string description { get; set; }

		public string imageSrc { get; set; }

		public string rewardDescription { get; set; }

		public double limit { get; set; }

		public DateTime startsAt { get; set; }

		public DateTime endsAt { get; set; }

		public DateTime lastProgressCheck { get; set; }

		public BountyObjective[] objectives { get; set; }

		public BountyReward[] rewards { get; set; }

		public dependants_Struct_Element[] dependants { get; set; }

		public prerequisites_Struct_Element[] prerequisites { get; set; }
	}

	public struct BountySku
	{
		public string id { get; set; }

		public string name { get; set; }

		public string slug { get; set; }
	}

	public struct BountyObjective
	{
		public struct category_Struct
		{
			public string id { get; set; }

			public string name { get; set; }

			public string slug { get; set; }
		}

		public struct publisher_Struct
		{
			public string id { get; set; }

			public string name { get; set; }
		}

		public string id { get; set; }

		public string name { get; set; }

		public string type { get; set; }

		public string condition { get; set; }

		public double count { get; set; }

		public string eventType { get; set; }

		public string eventCode { get; set; }

		public string nexusPurchaseObjectiveType { get; set; }

		public BountySku[] skus { get; set; }

		public category_Struct category { get; set; }

		public publisher_Struct publisher { get; set; }
	}

	public struct BountyReward
	{
		public string id { get; set; }

		public string name { get; set; }

		public string type { get; set; }

		public BountySku sku { get; set; }

		public double amount { get; set; }

		public string currency { get; set; }

		public string externalId { get; set; }
	}

	public struct BountyProgress
	{
		public struct completedObjectives_Struct_Element
		{
			public string objectiveGroupId { get; set; }

			public BountyObjectiveProgress[] objectives { get; set; }

			public BountyProgressReward[] rewards { get; set; }
		}

		public struct member_Struct
		{
			public string id { get; set; }

			public string playerId { get; set; }

			public string name { get; set; }

			public Code[] codes { get; set; }
		}

		public string id { get; set; }

		public bool completed { get; set; }

		public double completionCount { get; set; }

		public DateTime lastProgressCheck { get; set; }

		public string currentObjectiveGroupId { get; set; }

		public BountyObjectiveProgress[] currentObjectives { get; set; }

		public completedObjectives_Struct_Element[] completedObjectives { get; set; }

		public member_Struct member { get; set; }
	}

	public struct Code
	{
		public string code { get; set; }

		public bool isPrimary { get; set; }

		public bool isGenerated { get; set; }

		public bool isManaged { get; set; }
	}

	public struct BountyObjectiveProgress
	{
		public struct objective_Struct
		{
			public string id { get; set; }

			public string name { get; set; }

			public double count { get; set; }

			public string condition { get; set; }
		}

		public string id { get; set; }

		public bool completed { get; set; }

		public double count { get; set; }

		public objective_Struct objective { get; set; }
	}

	public struct BountyProgressReward
	{
		public string id { get; set; }

		public string name { get; set; }

		public string externalId { get; set; }

		public bool rewardCompleted { get; set; }

		public string rewardReferenceId { get; set; }
	}

	public struct Creator
	{
		public string id { get; set; }

		public string playerId { get; set; }

		public string name { get; set; }

		public string[] slugs { get; set; }
	}

	public struct CreatorGroupEvent
	{
		public string eventCode { get; set; }

		public string playerId { get; set; }

		public string referralCode { get; set; }
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

	public struct BountyError
	{
		public string code { get; set; }

		public string message { get; set; }
	}

	public delegate void ErrorDelegate(long ErrorCode);

	public struct GetBountiesRequestParams
	{
		public string groupId { get; set; }

		public int page { get; set; }

		public int pageSize { get; set; }
	}

	public struct GetBounties200Response
	{
		public string groupId { get; set; }

		public string groupName { get; set; }

		public int currentPage { get; set; }

		public int currentPageSize { get; set; }

		public int totalCount { get; set; }

		public Bounty[] bounties { get; set; }
	}

	public delegate void OnGetBounties200ResponseDelegate(GetBounties200Response Param0);

	public delegate void OnGetBounties400ResponseDelegate(BountyError Param0);

	public struct GetBountiesResponseCallbacks
	{
		public OnGetBounties200ResponseDelegate OnGetBounties200Response { get; set; }

		public OnGetBounties400ResponseDelegate OnGetBounties400Response { get; set; }
	}

	public struct GetBountyRequestParams
	{
		public string groupId { get; set; }

		public bool includeProgress { get; set; }

		public int page { get; set; }

		public int pageSize { get; set; }

		public string bountyId { get; set; }
	}

	public struct GetBounty200Response
	{
		public struct progress_Struct
		{
			public struct data_Struct_Element
			{
				public struct completedObjectives_Struct_Element
				{
					public string objectiveGroupId { get; set; }

					public BountyObjectiveProgress[] objectives { get; set; }

					public BountyProgressReward[] rewards { get; set; }
				}

				public struct member_Struct
				{
					public string id { get; set; }

					public string playerId { get; set; }

					public string name { get; set; }

					public Code[] codes { get; set; }
				}

				public string id { get; set; }

				public bool completed { get; set; }

				public double completionCount { get; set; }

				public DateTime lastProgressCheck { get; set; }

				public string currentObjectiveGroupId { get; set; }

				public BountyObjectiveProgress[] currentObjectives { get; set; }

				public BountyProgress.completedObjectives_Struct_Element[] completedObjectives { get; set; }

				public BountyProgress.member_Struct member { get; set; }

				public Creator creator { get; set; }
			}

			public int currentPage { get; set; }

			public int currentPageSize { get; set; }

			public int totalCount { get; set; }

			public data_Struct_Element[] data { get; set; }
		}

		public string groupId { get; set; }

		public string groupName { get; set; }

		public Bounty bounty { get; set; }

		public progress_Struct progress { get; set; }
	}

	public delegate void OnGetBounty200ResponseDelegate(GetBounty200Response Param0);

	public delegate void OnGetBounty400ResponseDelegate(BountyError Param0);

	public struct GetBountyResponseCallbacks
	{
		public OnGetBounty200ResponseDelegate OnGetBounty200Response { get; set; }

		public OnGetBounty400ResponseDelegate OnGetBounty400Response { get; set; }
	}

	public struct GetMemberBountiesByIDRequestParams
	{
		public string groupId { get; set; }

		public int page { get; set; }

		public int pageSize { get; set; }

		public string memberId { get; set; }
	}

	public struct GetMemberBountiesByID200Response
	{
		public struct progress_Struct_Element
		{
			public struct completedObjectives_Struct_Element
			{
				public string objectiveGroupId { get; set; }

				public BountyObjectiveProgress[] objectives { get; set; }

				public BountyProgressReward[] rewards { get; set; }
			}

			public struct member_Struct
			{
				public string id { get; set; }

				public string playerId { get; set; }

				public string name { get; set; }

				public Code[] codes { get; set; }
			}

			public string id { get; set; }

			public bool completed { get; set; }

			public double completionCount { get; set; }

			public DateTime lastProgressCheck { get; set; }

			public string currentObjectiveGroupId { get; set; }

			public BountyObjectiveProgress[] currentObjectives { get; set; }

			public BountyProgress.completedObjectives_Struct_Element[] completedObjectives { get; set; }

			public BountyProgress.member_Struct member { get; set; }

			public string name { get; set; }

			public double limit { get; set; }
		}

		public string groupId { get; set; }

		public string groupName { get; set; }

		public int currentPage { get; set; }

		public int currentPageSize { get; set; }

		public int totalCount { get; set; }

		public string memberId { get; set; }

		public string playerId { get; set; }

		public progress_Struct_Element[] progress { get; set; }
	}

	public delegate void OnGetMemberBountiesByID200ResponseDelegate(GetMemberBountiesByID200Response Param0);

	public delegate void OnGetMemberBountiesByID400ResponseDelegate(BountyError Param0);

	public struct GetMemberBountiesByIDResponseCallbacks
	{
		public OnGetMemberBountiesByID200ResponseDelegate OnGetMemberBountiesByID200Response { get; set; }

		public OnGetMemberBountiesByID400ResponseDelegate OnGetMemberBountiesByID400Response { get; set; }
	}

	public static IEnumerator StartGetBountiesRequest(GetBountiesRequestParams RequestParams, GetBountiesResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/";
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetBounties200Response != null)
			{
				GetBounties200Response param = JsonConvert.DeserializeObject<GetBounties200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetBounties200Response(param);
			}
			break;
		case 400L:
			if (ResponseCallback.OnGetBounties400Response != null)
			{
				BountyError param2 = JsonConvert.DeserializeObject<BountyError>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetBounties400Response(param2);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}

	public static IEnumerator StartGetBountyRequest(GetBountyRequestParams RequestParams, GetBountyResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/{bountyId}";
		text = text.Replace("{bountyId}", RequestParams.bountyId);
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		list.Add("includeProgress=" + RequestParams.includeProgress);
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetBounty200Response != null)
			{
				GetBounty200Response param = JsonConvert.DeserializeObject<GetBounty200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetBounty200Response(param);
			}
			break;
		case 400L:
			if (ResponseCallback.OnGetBounty400Response != null)
			{
				BountyError param2 = JsonConvert.DeserializeObject<BountyError>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetBounty400Response(param2);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}

	public static IEnumerator StartGetMemberBountiesByIDRequest(GetMemberBountiesByIDRequestParams RequestParams, GetMemberBountiesByIDResponseCallbacks ResponseCallback, ErrorDelegate ErrorCallback)
	{
		if (RequestParams.page > 9999 || RequestParams.page < 1 || RequestParams.pageSize > 100 || RequestParams.pageSize < 1)
		{
			yield break;
		}
		string text = SDKInitializer.ApiBaseUrl + "/member/id/{memberId}";
		text = text.Replace("{memberId}", RequestParams.memberId);
		List<string> list = new List<string>();
		if (RequestParams.groupId != "")
		{
			list.Add("groupId=" + RequestParams.groupId);
		}
		list.Add("page=" + RequestParams.page);
		list.Add("pageSize=" + RequestParams.pageSize);
		text += "?";
		text += string.Join("&", list);
		using UnityWebRequest webRequest = UnityWebRequest.Get(text);
		webRequest.SetRequestHeader("x-shared-secret", SDKInitializer.ApiKey);
		yield return webRequest.SendWebRequest();
		switch (webRequest.responseCode)
		{
		case 200L:
			if (ResponseCallback.OnGetMemberBountiesByID200Response != null)
			{
				GetMemberBountiesByID200Response param = JsonConvert.DeserializeObject<GetMemberBountiesByID200Response>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetMemberBountiesByID200Response(param);
			}
			break;
		case 400L:
			if (ResponseCallback.OnGetMemberBountiesByID400Response != null)
			{
				BountyError param2 = JsonConvert.DeserializeObject<BountyError>(webRequest.downloadHandler.text, new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				});
				ResponseCallback.OnGetMemberBountiesByID400Response(param2);
			}
			break;
		default:
			ErrorCallback?.Invoke(webRequest.responseCode);
			break;
		}
	}
}
