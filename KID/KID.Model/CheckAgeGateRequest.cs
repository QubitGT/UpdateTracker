using System;
using System.Runtime.Serialization;
using System.Text;
using KID.Client;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CheckAgeGateRequest")]
public class CheckAgeGateRequest
{
	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[DataMember(Name = "dateOfBirth", EmitDefaultValue = false)]
	[JsonConverter(typeof(OpenAPIDateConverter))]
	public DateTime DateOfBirth { get; set; }

	[DataMember(Name = "age", EmitDefaultValue = false)]
	public int Age { get; set; }

	[JsonConstructor]
	protected CheckAgeGateRequest()
	{
	}

	public CheckAgeGateRequest(string jurisdiction = null, DateTime dateOfBirth = default(DateTime), int age = 0)
	{
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for CheckAgeGateRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
		DateOfBirth = dateOfBirth;
		Age = age;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CheckAgeGateRequest {\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  DateOfBirth: ").Append(DateOfBirth).Append("\n");
		stringBuilder.Append("  Age: ").Append(Age).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
