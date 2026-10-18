using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "Challenge")]
public class Challenge
{
	[DataMember(Name = "type", IsRequired = true, EmitDefaultValue = true)]
	public ChallengeType Type { get; set; }

	[DataMember(Name = "challengeId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ChallengeId { get; set; }

	[DataMember(Name = "url", EmitDefaultValue = false)]
	public string Url { get; set; }

	[DataMember(Name = "oneTimePassword", EmitDefaultValue = false)]
	public string OneTimePassword { get; set; }

	[DataMember(Name = "childLiteAccessEnabled", IsRequired = true, EmitDefaultValue = true)]
	public bool ChildLiteAccessEnabled { get; set; }

	[JsonConstructor]
	protected Challenge()
	{
	}

	public Challenge(Guid challengeId = default(Guid), ChallengeType type = (ChallengeType)0, string url = null, string oneTimePassword = null, bool childLiteAccessEnabled = false)
	{
		ChallengeId = challengeId;
		Type = type;
		ChildLiteAccessEnabled = childLiteAccessEnabled;
		Url = url;
		OneTimePassword = oneTimePassword;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class Challenge {\n");
		stringBuilder.Append("  ChallengeId: ").Append(ChallengeId).Append("\n");
		stringBuilder.Append("  Type: ").Append(Type).Append("\n");
		stringBuilder.Append("  Url: ").Append(Url).Append("\n");
		stringBuilder.Append("  OneTimePassword: ").Append(OneTimePassword).Append("\n");
		stringBuilder.Append("  ChildLiteAccessEnabled: ").Append(ChildLiteAccessEnabled).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
