using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum VerificationStatus
{
	[EnumMember(Value = "PASS")]
	PASS = 1,
	[EnumMember(Value = "FAIL")]
	FAIL,
	[EnumMember(Value = "PENDING")]
	PENDING,
	[EnumMember(Value = "INCONCLUSIVE")]
	INCONCLUSIVE,
	[EnumMember(Value = "TIMED_OUT")]
	TIMEDOUT
}
