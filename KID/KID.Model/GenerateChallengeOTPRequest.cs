using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "GenerateChallengeOTPRequest")]
public class GenerateChallengeOTPRequest
{
	[DataMember(Name = "challengeId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ChallengeId { get; set; }

	[JsonConstructor]
	protected GenerateChallengeOTPRequest()
	{
	}

	public GenerateChallengeOTPRequest(Guid challengeId = default(Guid))
	{
		ChallengeId = challengeId;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GenerateChallengeOTPRequest {\n");
		stringBuilder.Append("  ChallengeId: ").Append(ChallengeId).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
