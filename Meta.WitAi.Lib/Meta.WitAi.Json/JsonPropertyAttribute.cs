using System;

namespace Meta.WitAi.Json;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class JsonPropertyAttribute : Attribute
{
	public string PropertyName { get; private set; }

	public object DefaultValue { get; private set; }

	public JsonPropertyAttribute()
	{
		PropertyName = null;
		DefaultValue = null;
	}

	public JsonPropertyAttribute(string propertyName)
	{
		PropertyName = propertyName;
		DefaultValue = null;
	}

	public JsonPropertyAttribute(string propertyName, object defaultValue)
	{
		PropertyName = propertyName;
		DefaultValue = defaultValue;
	}
}
