using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "GenerateChallengeOTPResponse")]
public class GenerateChallengeOTPResponse
{
	[DataMember(Name = "otp", IsRequired = true, EmitDefaultValue = true)]
	public string Otp { get; set; }

	[DataMember(Name = "expiresAt", IsRequired = true, EmitDefaultValue = true)]
	public string ExpiresAt { get; set; }

	[JsonConstructor]
	protected GenerateChallengeOTPResponse()
	{
	}

	public GenerateChallengeOTPResponse(string otp = null, string expiresAt = null)
	{
		if (otp == null)
		{
			throw new ArgumentNullException("otp is a required property for GenerateChallengeOTPResponse and cannot be null");
		}
		Otp = otp;
		if (expiresAt == null)
		{
			throw new ArgumentNullException("expiresAt is a required property for GenerateChallengeOTPResponse and cannot be null");
		}
		ExpiresAt = expiresAt;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GenerateChallengeOTPResponse {\n");
		stringBuilder.Append("  Otp: ").Append(Otp).Append("\n");
		stringBuilder.Append("  ExpiresAt: ").Append(ExpiresAt).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
