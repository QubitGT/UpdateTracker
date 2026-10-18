using System;

namespace Meta.WitAi.Json;

public class DateTimeConverter : JsonConverter
{
	public override bool CanRead => true;

	public override bool CanWrite => true;

	public override bool CanConvert(Type objectType)
	{
		return typeof(DateTime) == objectType;
	}

	public override object ReadJson(WitResponseNode serializer, Type objectType, object existingValue)
	{
		if (DateTime.TryParse(serializer.Value, out var result))
		{
			return result;
		}
		return existingValue;
	}

	public override WitResponseNode WriteJson(object existingValue)
	{
		DateTime dateTime = (DateTime)existingValue;
		return new WitResponseData(dateTime.ToLongDateString() + " " + dateTime.ToLongTimeString());
	}
}
