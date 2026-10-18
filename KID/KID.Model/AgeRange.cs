using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "AgeRange")]
public class AgeRange
{
	[DataMember(Name = "minAge", EmitDefaultValue = false)]
	public int MinAge { get; set; }

	[DataMember(Name = "maxAge", EmitDefaultValue = false)]
	public int MaxAge { get; set; }

	[DataMember(Name = "confidence", EmitDefaultValue = false)]
	public decimal Confidence { get; set; }

	public AgeRange(int minAge = 0, int maxAge = 0, decimal confidence = 0m)
	{
		MinAge = minAge;
		MaxAge = maxAge;
		Confidence = confidence;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class AgeRange {\n");
		stringBuilder.Append("  MinAge: ").Append(MinAge).Append("\n");
		stringBuilder.Append("  MaxAge: ").Append(MaxAge).Append("\n");
		stringBuilder.Append("  Confidence: ").Append(Confidence).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
