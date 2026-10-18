using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "CheckAgeGateResponse")]
public class CheckAgeGateResponse
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum StatusEnum
	{
		[EnumMember(Value = "PASS")]
		PASS = 1,
		[EnumMember(Value = "PROHIBITED")]
		PROHIBITED,
		[EnumMember(Value = "CHALLENGE")]
		CHALLENGE
	}

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public StatusEnum Status { get; set; }

	[DataMember(Name = "session", EmitDefaultValue = false)]
	public Session Session { get; set; }

	[DataMember(Name = "challenge", EmitDefaultValue = false)]
	public Challenge Challenge { get; set; }

	[JsonConstructor]
	protected CheckAgeGateResponse()
	{
	}

	public CheckAgeGateResponse(StatusEnum status = (StatusEnum)0, Session session = null, Challenge challenge = null)
	{
		Status = status;
		Session = session;
		Challenge = challenge;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CheckAgeGateResponse {\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  Session: ").Append(Session).Append("\n");
		stringBuilder.Append("  Challenge: ").Append(Challenge).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
