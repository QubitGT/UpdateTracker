using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateAgeVerificationRequest")]
public class CreateAgeVerificationRequest
{
	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[DataMember(Name = "locale", EmitDefaultValue = false)]
	public string Locale { get; set; }

	[DataMember(Name = "subject", EmitDefaultValue = false)]
	public VerificationSubject Subject { get; set; }

	[DataMember(Name = "criteria", IsRequired = true, EmitDefaultValue = true)]
	public AgeCriteria Criteria { get; set; }

	[DataMember(Name = "options", EmitDefaultValue = false)]
	public VerificationOptions Options { get; set; }

	[JsonConstructor]
	protected CreateAgeVerificationRequest()
	{
	}

	public CreateAgeVerificationRequest(string jurisdiction = null, string locale = null, VerificationSubject subject = null, AgeCriteria criteria = null, VerificationOptions options = null)
	{
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for CreateAgeVerificationRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
		if (criteria == null)
		{
			throw new ArgumentNullException("criteria is a required property for CreateAgeVerificationRequest and cannot be null");
		}
		Criteria = criteria;
		Locale = locale;
		Subject = subject;
		Options = options;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateAgeVerificationRequest {\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  Locale: ").Append(Locale).Append("\n");
		stringBuilder.Append("  Subject: ").Append(Subject).Append("\n");
		stringBuilder.Append("  Criteria: ").Append(Criteria).Append("\n");
		stringBuilder.Append("  Options: ").Append(Options).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
