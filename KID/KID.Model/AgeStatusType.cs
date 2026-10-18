using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum AgeStatusType
{
	[EnumMember(Value = "DIGITAL_MINOR")]
	DIGITALMINOR = 1,
	[EnumMember(Value = "DIGITAL_YOUTH")]
	DIGITALYOUTH,
	[EnumMember(Value = "LEGAL_ADULT")]
	LEGALADULT
}
