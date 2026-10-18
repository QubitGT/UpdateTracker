using System;

namespace Meta.WitAi.Json;

public abstract class JsonConverter
{
	public virtual bool CanRead { get; }

	public virtual bool CanWrite { get; }

	public abstract bool CanConvert(Type objectType);

	public virtual object ReadJson(WitResponseNode serializer, Type objectType, object existingValue)
	{
		return existingValue;
	}

	public virtual WitResponseNode WriteJson(object existingValue)
	{
		return null;
	}
}
