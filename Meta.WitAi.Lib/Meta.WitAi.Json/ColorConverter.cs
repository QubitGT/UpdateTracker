using System;
using UnityEngine;

namespace Meta.WitAi.Json;

public class ColorConverter : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return typeof(Color) == objectType;
	}

	public override object ReadJson(WitResponseNode serializer, Type objectType, object existingValue)
	{
		if (ColorUtility.TryParseHtmlString(serializer.Value, out var color))
		{
			return color;
		}
		return existingValue;
	}

	public override WitResponseNode WriteJson(object existingValue)
	{
		return new WitResponseData(ColorUtility.ToHtmlStringRGBA((Color)existingValue));
	}
}
