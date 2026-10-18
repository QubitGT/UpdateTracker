using System;
using System.Runtime.Serialization;
using System.Text;
using KID.Client;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "VerificationSubject")]
public class VerificationSubject
{
	[DataMember(Name = "email", EmitDefaultValue = false)]
	public string Email { get; set; }

	[DataMember(Name = "claimedAge", EmitDefaultValue = false)]
	public int ClaimedAge { get; set; }

	[DataMember(Name = "claimedDateOfBirth", EmitDefaultValue = false)]
	[JsonConverter(typeof(OpenAPIDateConverter))]
	public DateTime ClaimedDateOfBirth { get; set; }

	public VerificationSubject(string email = null, int claimedAge = 0, DateTime claimedDateOfBirth = default(DateTime))
	{
		Email = email;
		ClaimedAge = claimedAge;
		ClaimedDateOfBirth = claimedDateOfBirth;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class VerificationSubject {\n");
		stringBuilder.Append("  Email: ").Append(Email).Append("\n");
		stringBuilder.Append("  ClaimedAge: ").Append(ClaimedAge).Append("\n");
		stringBuilder.Append("  ClaimedDateOfBirth: ").Append(ClaimedDateOfBirth).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
