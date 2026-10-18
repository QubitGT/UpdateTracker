using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum VerificationStatusV2
{
	[EnumMember(Value = "PASS")]
	PASS = 1,
	[EnumMember(Value = "FAIL")]
	FAIL,
	[EnumMember(Value = "PENDING")]
	PENDING,
	[EnumMember(Value = "IN_PROGRESS")]
	INPROGRESS
}
