using System.Collections.Generic;
using Meta.Voice;
using Meta.WitAi.Configuration;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Data.Entities;
using Meta.WitAi.Data.Info;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;

namespace Meta.WitAi;

public static class WitRequestFactory
{
	private static VoiceServiceRequestOptions.QueryParam QueryParam(string key, string value)
	{
		return new VoiceServiceRequestOptions.QueryParam
		{
			key = key,
			value = value
		};
	}

	private static void HandleWitRequestOptions(WitRequestOptions requestOptions, IDynamicEntitiesProvider[] additionalEntityProviders)
	{
		WitResponseClass witResponseClass = new WitResponseClass();
		bool flag = false;
		if (additionalEntityProviders != null)
		{
			for (int i = 0; i < additionalEntityProviders.Length; i++)
			{
				foreach (WitDynamicEntity dynamicEntity in additionalEntityProviders[i].GetDynamicEntities())
				{
					flag = true;
					MergeEntities(witResponseClass, dynamicEntity);
				}
			}
		}
		if (DynamicEntityKeywordRegistry.HasDynamicEntityRegistry)
		{
			foreach (WitDynamicEntity dynamicEntity2 in DynamicEntityKeywordRegistry.Instance.GetDynamicEntities())
			{
				flag = true;
				MergeEntities(witResponseClass, dynamicEntity2);
			}
		}
		if (requestOptions != null && requestOptions.dynamicEntities != null)
		{
			foreach (WitDynamicEntity dynamicEntity3 in requestOptions.dynamicEntities.GetDynamicEntities())
			{
				flag = true;
				MergeEntities(witResponseClass, dynamicEntity3);
			}
		}
		if (flag)
		{
			requestOptions.QueryParams["entities"] = witResponseClass.ToString();
		}
	}

	private static void MergeEntities(WitResponseClass entities, WitDynamicEntity providerEntity)
	{
		if (!entities.HasChild(providerEntity.entity))
		{
			entities[providerEntity.entity] = new WitResponseArray();
		}
		WitResponseNode witResponseNode = entities[providerEntity.entity];
		Dictionary<string, WitResponseClass> dictionary = new Dictionary<string, WitResponseClass>();
		new HashSet<string>();
		WitResponseArray asArray = witResponseNode.AsArray;
		for (int i = 0; i < asArray.Count; i++)
		{
			WitResponseClass asObject = asArray[i].AsObject;
			string value = asObject["keyword"].Value;
			if (!dictionary.ContainsKey(value))
			{
				dictionary[value] = asObject;
			}
		}
		foreach (WitEntityKeywordInfo keyword in providerEntity.keywords)
		{
			if (dictionary.TryGetValue(keyword.keyword, out var value2))
			{
				foreach (string synonym in keyword.synonyms)
				{
					value2["synonyms"].Add(synonym);
				}
			}
			else
			{
				value2 = JsonConvert.SerializeToken(keyword).AsObject;
				dictionary[keyword.keyword] = value2;
				witResponseNode.Add(value2);
			}
		}
	}

	public static WitRequestOptions GetSetupOptions(WitConfiguration configuration, WitRequestOptions newOptions, IDynamicEntitiesProvider[] additionalDynamicEntities)
	{
		WitRequestOptions witRequestOptions = newOptions ?? new WitRequestOptions();
		if (-1 != witRequestOptions.nBestIntents)
		{
			witRequestOptions.QueryParams["n"] = witRequestOptions.nBestIntents.ToString();
		}
		string versionTag = configuration.GetVersionTag();
		if (!string.IsNullOrEmpty(versionTag))
		{
			witRequestOptions.QueryParams["tag"] = versionTag;
		}
		HandleWitRequestOptions(witRequestOptions, additionalDynamicEntities);
		return witRequestOptions;
	}

	public static VoiceServiceRequest CreateMessageRequest(this WitConfiguration config, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents, IDynamicEntitiesProvider[] additionalEntityProviders = null)
	{
		WitRequestOptions setupOptions = GetSetupOptions(config, requestOptions, additionalEntityProviders);
		return new WitUnityRequest(config, NLPRequestInputType.Text, setupOptions, requestEvents);
	}

	public static WitRequest CreateSpeechRequest(this WitConfiguration config, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents, IDynamicEntitiesProvider[] additionalEntityProviders = null)
	{
		WitRequestOptions setupOptions = GetSetupOptions(config, requestOptions, additionalEntityProviders);
		string speech = config.GetEndpointInfo().Speech;
		return new WitRequest(config, speech, setupOptions, requestEvents);
	}

	public static WitRequest CreateDictationRequest(this WitConfiguration config, WitRequestOptions requestOptions, VoiceServiceRequestEvents requestEvents = null)
	{
		WitRequestOptions setupOptions = GetSetupOptions(config, requestOptions, null);
		string dictation = config.GetEndpointInfo().Dictation;
		return new WitRequest(config, dictation, setupOptions, requestEvents);
	}
}
