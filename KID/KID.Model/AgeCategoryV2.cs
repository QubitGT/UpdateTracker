using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum AgeCategoryV2
{
	[EnumMember(Value = "digital-minor")]
	DigitalMinor = 1,
	[EnumMember(Value = "digital-youth")]
	DigitalYouth,
	[EnumMember(Value = "adult")]
	Adult
}
