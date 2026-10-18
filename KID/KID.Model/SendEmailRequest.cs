using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "SendEmailRequest")]
public class SendEmailRequest
{
	[DataMember(Name = "challengeId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ChallengeId { get; set; }

	[DataMember(Name = "email", EmitDefaultValue = false)]
	public string Email { get; set; }

	[DataMember(Name = "locale", EmitDefaultValue = false)]
	public string Locale { get; set; }

	[JsonConstructor]
	protected SendEmailRequest()
	{
	}

	public SendEmailRequest(Guid challengeId = default(Guid), string email = null, string locale = null)
	{
		ChallengeId = challengeId;
		Email = email;
		Locale = locale;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class SendEmailRequest {\n");
		stringBuilder.Append("  ChallengeId: ").Append(ChallengeId).Append("\n");
		stringBuilder.Append("  Email: ").Append(Email).Append("\n");
		stringBuilder.Append("  Locale: ").Append(Locale).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
