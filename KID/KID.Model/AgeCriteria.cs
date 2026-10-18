using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "AgeCriteria")]
public class AgeCriteria
{
	[DataMember(Name = "ageCategory", EmitDefaultValue = false)]
	public AgeCategory? AgeCategory { get; set; }

	[DataMember(Name = "age", EmitDefaultValue = false)]
	public int Age { get; set; }

	public AgeCriteria(int age = 0, AgeCategory? ageCategory = null)
	{
		Age = age;
		AgeCategory = ageCategory;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class AgeCriteria {\n");
		stringBuilder.Append("  Age: ").Append(Age).Append("\n");
		stringBuilder.Append("  AgeCategory: ").Append(AgeCategory).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
