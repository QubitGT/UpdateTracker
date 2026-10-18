using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "AgeRangeV2")]
public class AgeRangeV2
{
	[DataMember(Name = "low", EmitDefaultValue = false)]
	public int Low { get; set; }

	[DataMember(Name = "high", EmitDefaultValue = false)]
	public int High { get; set; }

	[DataMember(Name = "confidence", EmitDefaultValue = false)]
	public decimal Confidence { get; set; }

	public AgeRangeV2(int low = 0, int high = 0, decimal confidence = 0m)
	{
		Low = low;
		High = high;
		Confidence = confidence;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class AgeRangeV2 {\n");
		stringBuilder.Append("  Low: ").Append(Low).Append("\n");
		stringBuilder.Append("  High: ").Append(High).Append("\n");
		stringBuilder.Append("  Confidence: ").Append(Confidence).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
