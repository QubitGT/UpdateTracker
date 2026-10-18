using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateParentalConsentChallengeRequest")]
public class CreateParentalConsentChallengeRequest
{
	[DataMember(Name = "scenarioId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ScenarioId { get; set; }

	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[JsonConstructor]
	protected CreateParentalConsentChallengeRequest()
	{
	}

	public CreateParentalConsentChallengeRequest(Guid scenarioId = default(Guid), string jurisdiction = null)
	{
		ScenarioId = scenarioId;
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for CreateParentalConsentChallengeRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateParentalConsentChallengeRequest {\n");
		stringBuilder.Append("  ScenarioId: ").Append(ScenarioId).Append("\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
