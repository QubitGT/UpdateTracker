using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateAgeAssuranceVerificationRequest")]
public class CreateAgeAssuranceVerificationRequest
{
	[DataMember(Name = "ageCategory", EmitDefaultValue = false)]
	public AgeCategory? AgeCategory { get; set; }

	[DataMember(Name = "email", EmitDefaultValue = false)]
	public string Email { get; set; }

	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[DataMember(Name = "locale", EmitDefaultValue = false)]
	public string Locale { get; set; }

	[DataMember(Name = "age", EmitDefaultValue = false)]
	public int Age { get; set; }

	[DataMember(Name = "disableInstructions", EmitDefaultValue = true)]
	public bool DisableInstructions { get; set; }

	[JsonConstructor]
	protected CreateAgeAssuranceVerificationRequest()
	{
	}

	public CreateAgeAssuranceVerificationRequest(string email = null, string jurisdiction = null, string locale = null, int age = 0, AgeCategory? ageCategory = null, bool disableInstructions = false)
	{
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for CreateAgeAssuranceVerificationRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
		Email = email;
		Locale = locale;
		Age = age;
		AgeCategory = ageCategory;
		DisableInstructions = disableInstructions;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateAgeAssuranceVerificationRequest {\n");
		stringBuilder.Append("  Email: ").Append(Email).Append("\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  Locale: ").Append(Locale).Append("\n");
		stringBuilder.Append("  Age: ").Append(Age).Append("\n");
		stringBuilder.Append("  AgeCategory: ").Append(AgeCategory).Append("\n");
		stringBuilder.Append("  DisableInstructions: ").Append(DisableInstructions).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
