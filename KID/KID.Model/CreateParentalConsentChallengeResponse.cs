using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateParentalConsentChallengeResponse")]
public class CreateParentalConsentChallengeResponse
{
	[DataMember(Name = "challengeId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ChallengeId { get; set; }

	[DataMember(Name = "longUrl", IsRequired = true, EmitDefaultValue = true)]
	public string LongUrl { get; set; }

	[JsonConstructor]
	protected CreateParentalConsentChallengeResponse()
	{
	}

	public CreateParentalConsentChallengeResponse(Guid challengeId = default(Guid), string longUrl = null)
	{
		ChallengeId = challengeId;
		if (longUrl == null)
		{
			throw new ArgumentNullException("longUrl is a required property for CreateParentalConsentChallengeResponse and cannot be null");
		}
		LongUrl = longUrl;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateParentalConsentChallengeResponse {\n");
		stringBuilder.Append("  ChallengeId: ").Append(ChallengeId).Append("\n");
		stringBuilder.Append("  LongUrl: ").Append(LongUrl).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
