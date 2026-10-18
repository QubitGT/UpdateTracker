using System;
using System.Collections.Generic;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

[Serializable]
public class TTSWitVoiceSettings : TTSVoiceSettings
{
	public string voice = "Charlie";

	public string style = "default";

	[Range(50f, 200f)]
	public int speed = 100;

	[Range(25f, 200f)]
	public int pitch = 100;

	private string _uniqueId;

	private Dictionary<string, string> _encoded = new Dictionary<string, string>();

	public override string UniqueId
	{
		get
		{
			if (string.IsNullOrEmpty(_uniqueId))
			{
				RefreshUniqueId();
			}
			return _uniqueId;
		}
	}

	public override Dictionary<string, string> EncodedValues
	{
		get
		{
			if (_encoded.Keys.Count == 0)
			{
				RefreshEncodedValues();
			}
			return _encoded;
		}
	}

	public void RefreshUniqueId()
	{
		_uniqueId = $"{voice}|{style}|{speed:00}|{pitch:00}";
	}

	public void RefreshEncodedValues()
	{
		_encoded.Clear();
		_encoded[WitConstants.TTS_VOICE] = (string.IsNullOrEmpty(voice) ? "Charlie" : voice);
		_encoded[WitConstants.TTS_STYLE] = (string.IsNullOrEmpty(style) ? "default" : style);
		int num = Mathf.Clamp(speed, 50, 200);
		if (num != 100)
		{
			_encoded[WitConstants.TTS_SPEED] = num.ToString();
		}
		num = Mathf.Clamp(pitch, 25, 200);
		if (num != 100)
		{
			_encoded[WitConstants.TTS_PITCH] = num.ToString();
		}
	}

	public static bool CanDecode(WitResponseNode responseNode)
	{
		WitResponseClass witResponseClass = responseNode?.AsObject;
		if (witResponseClass != null && witResponseClass.HasChild("q"))
		{
			return witResponseClass.HasChild(WitConstants.TTS_VOICE);
		}
		return false;
	}

	public override bool SerializeObject(WitResponseClass jsonObject)
	{
		RefreshEncodedValues();
		Dictionary<string, string> encodedValues = EncodedValues;
		if (encodedValues == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, string> item in encodedValues)
		{
			jsonObject[item.Key] = new WitResponseData(item.Value);
		}
		return true;
	}

	public override bool DeserializeObject(WitResponseClass jsonObject)
	{
		voice = DecodeString(jsonObject, WitConstants.TTS_VOICE, "Charlie");
		style = DecodeString(jsonObject, WitConstants.TTS_STYLE, "default");
		speed = DecodeInt(jsonObject, WitConstants.TTS_SPEED, 100, 50, 200);
		pitch = DecodeInt(jsonObject, WitConstants.TTS_PITCH, 100, 25, 200);
		RefreshUniqueId();
		RefreshEncodedValues();
		SettingsId = UniqueId;
		return !string.IsNullOrEmpty(voice);
	}

	private string DecodeString(WitResponseClass responseClass, string id, string defaultValue)
	{
		if (responseClass.HasChild(id))
		{
			return responseClass[id];
		}
		return defaultValue;
	}

	private int DecodeInt(WitResponseClass responseClass, string id, int defaultValue, int minValue, int maxValue)
	{
		if (responseClass.HasChild(id))
		{
			return Mathf.Clamp(responseClass[id].AsInt, minValue, maxValue);
		}
		return defaultValue;
	}
}
