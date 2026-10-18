using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum AgeCategory
{
	[EnumMember(Value = "DIGITAL_YOUTH_OR_ADULT")]
	DIGITALYOUTHORADULT = 1,
	[EnumMember(Value = "ADULT")]
	ADULT
}
