using System;
using System.Runtime.Serialization;
using System.Text;
using KID.Client;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateVerificationRequest")]
public class CreateVerificationRequest
{
	[DataMember(Name = "scenarioId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ScenarioId { get; set; }

	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[DataMember(Name = "email", EmitDefaultValue = false)]
	public string Email { get; set; }

	[DataMember(Name = "criteria", EmitDefaultValue = false)]
	public AgeCriteria Criteria { get; set; }

	[DataMember(Name = "claimedDateOfBirth", EmitDefaultValue = false)]
	[JsonConverter(typeof(OpenAPIDateConverter))]
	public DateTime ClaimedDateOfBirth { get; set; }

	[DataMember(Name = "claimedAge", EmitDefaultValue = false)]
	public int ClaimedAge { get; set; }

	[JsonConstructor]
	protected CreateVerificationRequest()
	{
	}

	public CreateVerificationRequest(Guid scenarioId = default(Guid), string jurisdiction = null, string email = null, AgeCriteria criteria = null, DateTime claimedDateOfBirth = default(DateTime), int claimedAge = 0)
	{
		ScenarioId = scenarioId;
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for CreateVerificationRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
		Email = email;
		Criteria = criteria;
		ClaimedDateOfBirth = claimedDateOfBirth;
		ClaimedAge = claimedAge;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateVerificationRequest {\n");
		stringBuilder.Append("  ScenarioId: ").Append(ScenarioId).Append("\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  Email: ").Append(Email).Append("\n");
		stringBuilder.Append("  Criteria: ").Append(Criteria).Append("\n");
		stringBuilder.Append("  ClaimedDateOfBirth: ").Append(ClaimedDateOfBirth).Append("\n");
		stringBuilder.Append("  ClaimedAge: ").Append(ClaimedAge).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
