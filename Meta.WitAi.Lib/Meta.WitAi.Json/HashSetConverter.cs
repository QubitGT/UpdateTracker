using System;
using System.Collections.Generic;

namespace Meta.WitAi.Json;

public class HashSetConverter<T> : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return typeof(HashSet<T>) == objectType;
	}

	public override object ReadJson(WitResponseNode serializer, Type objectType, object existingValue)
	{
		WitResponseArray asArray = serializer.AsArray;
		HashSet<T> hashSet = new HashSet<T>();
		foreach (WitResponseNode item in asArray)
		{
			hashSet.Add(item.Cast<T>());
		}
		return hashSet;
	}

	public override WitResponseNode WriteJson(object existingValue)
	{
		WitResponseArray witResponseArray = new WitResponseArray();
		if (!(existingValue is HashSet<T> hashSet))
		{
			return witResponseArray;
		}
		foreach (T item in hashSet)
		{
			witResponseArray.Add(new WitResponseData(item.ToString()));
		}
		return witResponseArray;
	}
}
