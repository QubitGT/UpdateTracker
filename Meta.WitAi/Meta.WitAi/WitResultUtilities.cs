using Meta.WitAi.Data.Entities;
using Meta.WitAi.Data.Intents;
using Meta.WitAi.Json;

namespace Meta.WitAi;

public static class WitResultUtilities
{
	public const string WIT_KEY_TRANSCRIPTION = "text";

	public const string WIT_KEY_INTENTS = "intents";

	public const string WIT_KEY_ENTITIES = "entities";

	public const string WIT_KEY_TRAITS = "traits";

	public const string WIT_KEY_FINAL = "is_final";

	public const string WIT_PARTIAL_RESPONSE = "partial_response";

	public const string WIT_RESPONSE = "response";

	public const string WIT_STATUS_CODE = "code";

	public const string WIT_ERROR = "error";

	public static int GetStatusCode(this WitResponseNode witResponse)
	{
		if (!(null != witResponse) || !(witResponse.AsObject != null) || !witResponse.AsObject.HasChild("code"))
		{
			return 200;
		}
		return witResponse["code"].AsInt;
	}

	public static string GetError(this WitResponseNode witResponse)
	{
		if (!(null != witResponse) || !(witResponse.AsObject != null) || !witResponse.AsObject.HasChild("error"))
		{
			return string.Empty;
		}
		return witResponse["error"].Value;
	}

	public static string GetTranscription(this WitResponseNode witResponse)
	{
		if (!(null != witResponse) || !(witResponse.AsObject != null) || !witResponse.AsObject.HasChild("text"))
		{
			return string.Empty;
		}
		return witResponse["text"].Value;
	}

	public static WitResponseNode SafeGet(this WitResponseNode witResponse, string key)
	{
		WitResponseClass witResponseClass = witResponse?.AsObject;
		if (!(witResponseClass != null) || !witResponseClass.HasChild(key))
		{
			return null;
		}
		return witResponseClass[key];
	}

	public static string GetRequestId(this WitResponseNode witResponse)
	{
		return witResponse?["client_request_id"].Value ?? string.Empty;
	}

	public static string GetClientUserId(this WitResponseNode witResponse)
	{
		string text = witResponse?["client_user_id"].Value ?? string.Empty;
		if (string.IsNullOrEmpty(text))
		{
			return "unknown";
		}
		return text;
	}

	public static string GetResponseType(this WitResponseNode witResponse)
	{
		return witResponse?["type"];
	}

	public static WitResponseClass GetResponse(this WitResponseNode witResponse)
	{
		object obj = witResponse?.GetFinalResponse();
		if (obj == null)
		{
			if ((object)witResponse == null)
			{
				return null;
			}
			obj = witResponse.GetPartialResponse();
		}
		return (WitResponseClass)obj;
	}

	public static WitResponseClass GetFinalResponse(this WitResponseNode witResponse)
	{
		return witResponse?.SafeGet("response")?.AsObject;
	}

	public static WitResponseClass GetPartialResponse(this WitResponseNode witResponse)
	{
		return witResponse?.SafeGet("partial_response")?.AsObject;
	}

	public static bool GetIsFinal(this WitResponseNode witResponse)
	{
		return (witResponse?.SafeGet("is_final")?.AsBool).GetValueOrDefault();
	}

	public static bool GetIsNlpPartial(this WitResponseNode witResponse)
	{
		return string.Equals(witResponse?.GetResponseType(), "PARTIAL_UNDERSTANDING");
	}

	public static bool GetIsNlpFinal(this WitResponseNode witResponse)
	{
		return string.Equals(witResponse?.GetResponseType(), "FINAL_UNDERSTANDING");
	}

	public static bool GetIsTranscriptionPartial(this WitResponseNode witResponse)
	{
		if (string.Equals(witResponse?.GetResponseType(), "PARTIAL_TRANSCRIPTION"))
		{
			return !string.IsNullOrEmpty(witResponse["text"]);
		}
		return false;
	}

	public static bool GetIsTranscriptionFinal(this WitResponseNode witResponse)
	{
		if (string.Equals(witResponse?.GetResponseType(), "FINAL_TRANSCRIPTION"))
		{
			return !string.IsNullOrEmpty(witResponse["text"]);
		}
		return false;
	}

	public static bool GetHasTranscription(this WitResponseNode witResponse)
	{
		string a = witResponse?.GetResponseType();
		if (string.Equals(a, "PARTIAL_TRANSCRIPTION") || string.Equals(a, "FINAL_TRANSCRIPTION"))
		{
			return !string.IsNullOrEmpty(witResponse["text"]);
		}
		return false;
	}

	private static WitResponseArray GetArray(WitResponseNode witResponse, string key)
	{
		return witResponse?.SafeGet(key)?.AsArray;
	}

	public static WitEntityData AsWitEntity(this WitResponseNode witResponse)
	{
		return new WitEntityData(witResponse);
	}

	public static WitEntityFloatData AsWitFloatEntity(this WitResponseNode witResponse)
	{
		return new WitEntityFloatData(witResponse);
	}

	public static WitEntityIntData AsWitIntEntity(this WitResponseNode witResponse)
	{
		return new WitEntityIntData(witResponse);
	}

	public static string GetFirstEntityValue(this WitResponseNode witResponse, string name)
	{
		return witResponse?["entities"]?[name]?[0]?["value"]?.Value;
	}

	public static string[] GetAllEntityValues(this WitResponseNode witResponse, string name)
	{
		string[] array = new string[(witResponse?["entities"]?[name]?.Count).GetValueOrDefault()];
		for (int i = 0; i < witResponse?["entities"]?[name]?.Count; i++)
		{
			array[i] = witResponse?["entities"]?[name]?[i]?["value"]?.Value;
		}
		return array;
	}

	public static WitResponseNode GetFirstEntity(this WitResponseNode witResponse, string name)
	{
		return witResponse?["entities"]?[name][0];
	}

	public static WitEntityData GetFirstWitEntity(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		if ((object)witResponseArray == null || witResponseArray.Count <= 0)
		{
			return null;
		}
		return witResponseArray[0].AsWitEntity();
	}

	public static WitEntityIntData GetFirstWitIntEntity(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		if ((object)witResponseArray == null || witResponseArray.Count <= 0)
		{
			return null;
		}
		return witResponseArray[0].AsWitIntEntity();
	}

	public static int GetFirstWitIntValue(this WitResponseNode witResponse, string name, int defaultValue)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		if (null == witResponseArray || witResponseArray.Count == 0)
		{
			return defaultValue;
		}
		return witResponseArray[0].AsWitIntEntity().value;
	}

	public static WitEntityFloatData GetFirstWitFloatEntity(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		if ((object)witResponseArray == null || witResponseArray.Count <= 0)
		{
			return null;
		}
		return witResponseArray[0].AsWitFloatEntity();
	}

	public static float GetFirstWitFloatValue(this WitResponseNode witResponse, string name, float defaultValue)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		if (null == witResponseArray || witResponseArray.Count == 0)
		{
			return defaultValue;
		}
		return witResponseArray[0].AsWitFloatEntity().value;
	}

	public static WitEntityData[] GetEntities(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		WitEntityData[] array = new WitEntityData[witResponseArray?.Count ?? 0];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = witResponseArray[i].AsWitEntity();
		}
		return array;
	}

	public static int EntityCount(this WitResponseNode response)
	{
		return (response?["entities"]?.AsArray?.Count).GetValueOrDefault();
	}

	public static WitEntityFloatData[] GetFloatEntities(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		WitEntityFloatData[] array = new WitEntityFloatData[witResponseArray?.Count ?? 0];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = witResponseArray[i].AsWitFloatEntity();
		}
		return array;
	}

	public static WitEntityIntData[] GetIntEntities(this WitResponseNode witResponse, string name)
	{
		WitResponseArray witResponseArray = witResponse?["entities"]?[name].AsArray;
		WitEntityIntData[] array = new WitEntityIntData[witResponseArray?.Count ?? 0];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = witResponseArray[i].AsWitIntEntity();
		}
		return array;
	}

	public static WitIntentData AsWitIntent(this WitResponseNode witResponse)
	{
		return new WitIntentData(witResponse);
	}

	public static string GetIntentName(this WitResponseNode witResponse)
	{
		WitResponseNode firstIntent = witResponse.GetFirstIntent();
		if (!(firstIntent == null))
		{
			return firstIntent["name"]?.Value;
		}
		return null;
	}

	public static WitResponseNode GetFirstIntent(this WitResponseNode witResponse)
	{
		WitResponseArray array = GetArray(witResponse, "intents");
		if (!(array == null) && array.Count != 0)
		{
			return array[0];
		}
		return null;
	}

	public static WitIntentData GetFirstIntentData(this WitResponseNode witResponse)
	{
		return witResponse.GetFirstIntent()?.AsWitIntent();
	}

	public static WitIntentData[] GetIntents(this WitResponseNode witResponse)
	{
		WitResponseArray array = GetArray(witResponse, "intents");
		WitIntentData[] array2 = new WitIntentData[array?.Count ?? 0];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array[i].AsWitIntent();
		}
		return array2;
	}

	public static string GetPathValue(this WitResponseNode response, string path)
	{
		string[] array = path.Trim('.').Split('.');
		WitResponseNode witResponseNode = response;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = SplitArrays(array2[i]);
			witResponseNode = witResponseNode[array3[0]];
			for (int j = 1; j < array3.Length; j++)
			{
				witResponseNode = witResponseNode[int.Parse(array3[j])];
			}
		}
		return witResponseNode.Value;
	}

	public static void SetString(this WitResponseNode response, string path, string value)
	{
		string[] array = path.Trim('.').Split('.');
		WitResponseNode witResponseNode = response;
		int i;
		for (i = 0; i < array.Length - 1; i++)
		{
			string[] array2 = SplitArrays(array[i]);
			witResponseNode = witResponseNode[array2[0]];
			for (int j = 1; j < array2.Length; j++)
			{
				witResponseNode = witResponseNode[int.Parse(array2[j])];
			}
		}
		witResponseNode[array[i]] = value;
	}

	public static void RemovePath(this WitResponseNode response, string path)
	{
		string[] array = path.Trim('.').Split('.');
		WitResponseNode witResponseNode = response;
		WitResponseNode witResponseNode2 = null;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = SplitArrays(array2[i]);
			witResponseNode2 = witResponseNode;
			witResponseNode = witResponseNode[array3[0]];
			for (int j = 1; j < array3.Length; j++)
			{
				witResponseNode = witResponseNode[int.Parse(array3[j])];
			}
		}
		if (null != witResponseNode2)
		{
			witResponseNode2.Remove(witResponseNode);
		}
	}

	public static WitResponseReference GetWitResponseReference(string path)
	{
		string[] array = path.Trim('.').Split('.');
		WitResponseReference witResponseReference = new WitResponseReference
		{
			path = path
		};
		WitResponseReference witResponseReference2 = witResponseReference;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = SplitArrays(array2[i]);
			ObjectNodeReference objectNodeReference = new ObjectNodeReference
			{
				path = path
			};
			objectNodeReference.key = array3[0];
			witResponseReference2.child = objectNodeReference;
			witResponseReference2 = objectNodeReference;
			for (int j = 1; j < array3.Length; j++)
			{
				ArrayNodeReference arrayNodeReference = new ArrayNodeReference
				{
					path = path
				};
				arrayNodeReference.index = int.Parse(array3[j]);
				witResponseReference2.child = arrayNodeReference;
				witResponseReference2 = arrayNodeReference;
			}
		}
		return witResponseReference;
	}

	public static string GetCodeFromPath(string path)
	{
		string[] array = path.Trim('.').Split('.');
		string text = "witResponse";
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = SplitArrays(array2[i]);
			text = text + "[\"" + array3[0] + "\"]";
			for (int j = 1; j < array3.Length; j++)
			{
				text = text + "[" + array3[j] + "]";
			}
		}
		return text + ".Value";
	}

	private static string[] SplitArrays(string nodeName)
	{
		string[] array = nodeName.Split('[');
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim(']');
		}
		return array;
	}

	public static string GetTraitValue(this WitResponseNode witResponse, string name)
	{
		return witResponse?["traits"]?[name]?[0]?["value"]?.Value;
	}
}
