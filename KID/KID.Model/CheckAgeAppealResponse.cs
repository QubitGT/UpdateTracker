using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "CheckAgeAppealResponse")]
public class CheckAgeAppealResponse
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum StatusEnum
	{
		[EnumMember(Value = "PASS")]
		PASS = 1,
		[EnumMember(Value = "FAIL")]
		FAIL,
		[EnumMember(Value = "CHALLENGE")]
		CHALLENGE
	}

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public StatusEnum Status { get; set; }

	[DataMember(Name = "url", EmitDefaultValue = false)]
	public string Url { get; set; }

	[JsonConstructor]
	protected CheckAgeAppealResponse()
	{
	}

	public CheckAgeAppealResponse(StatusEnum status = (StatusEnum)0, string url = null)
	{
		Status = status;
		Url = url;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CheckAgeAppealResponse {\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  Url: ").Append(Url).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
