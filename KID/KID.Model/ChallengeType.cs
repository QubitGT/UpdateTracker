using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum ChallengeType
{
	[EnumMember(Value = "CHALLENGE_PARENTAL_CONSENT")]
	PARENTALCONSENT = 1,
	[EnumMember(Value = "CHALLENGE_SESSION_UPGRADE")]
	SESSIONUPGRADE
}
