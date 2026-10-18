using System;

namespace Meta.WitAi.Json;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class JsonIgnoreAttribute : Attribute
{
}
