using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "ShouldDisplayAgeGateResponse")]
public class ShouldDisplayAgeGateResponse
{
	[DataMember(Name = "shouldDisplay", IsRequired = true, EmitDefaultValue = true)]
	public bool ShouldDisplay { get; set; }

	[DataMember(Name = "ageAssuranceRequired", IsRequired = true, EmitDefaultValue = true)]
	public bool AgeAssuranceRequired { get; set; }

	[DataMember(Name = "digitalConsentAge", IsRequired = true, EmitDefaultValue = true)]
	public int DigitalConsentAge { get; set; }

	[DataMember(Name = "civilAge", IsRequired = true, EmitDefaultValue = true)]
	public int CivilAge { get; set; }

	[JsonConstructor]
	protected ShouldDisplayAgeGateResponse()
	{
	}

	public ShouldDisplayAgeGateResponse(bool shouldDisplay = false, bool ageAssuranceRequired = false, int digitalConsentAge = 0, int civilAge = 0)
	{
		ShouldDisplay = shouldDisplay;
		AgeAssuranceRequired = ageAssuranceRequired;
		DigitalConsentAge = digitalConsentAge;
		CivilAge = civilAge;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class ShouldDisplayAgeGateResponse {\n");
		stringBuilder.Append("  ShouldDisplay: ").Append(ShouldDisplay).Append("\n");
		stringBuilder.Append("  AgeAssuranceRequired: ").Append(AgeAssuranceRequired).Append("\n");
		stringBuilder.Append("  DigitalConsentAge: ").Append(DigitalConsentAge).Append("\n");
		stringBuilder.Append("  CivilAge: ").Append(CivilAge).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
