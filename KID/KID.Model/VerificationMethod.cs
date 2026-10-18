using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[JsonConverter(typeof(StringEnumConverter))]
public enum VerificationMethod
{
	[EnumMember(Value = "age-estimation")]
	AgeEstimation = 1,
	[EnumMember(Value = "id-document")]
	IdDocument,
	[EnumMember(Value = "credit-card")]
	CreditCard,
	[EnumMember(Value = "personal-details")]
	PersonalDetails,
	[EnumMember(Value = "kws")]
	Kws,
	[EnumMember(Value = "age-attestation")]
	AgeAttestation
}
