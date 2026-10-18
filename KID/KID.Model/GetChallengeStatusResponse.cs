using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "GetChallengeStatusResponse")]
public class GetChallengeStatusResponse
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum StatusEnum
	{
		[EnumMember(Value = "PASS")]
		PASS = 1,
		[EnumMember(Value = "FAIL")]
		FAIL,
		[EnumMember(Value = "PENDING")]
		PENDING,
		[EnumMember(Value = "IN_PROGRESS")]
		INPROGRESS
	}

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public StatusEnum Status { get; set; }

	[DataMember(Name = "sessionId", EmitDefaultValue = false)]
	public Guid SessionId { get; set; }

	[DataMember(Name = "approverEmail", EmitDefaultValue = false)]
	public string ApproverEmail { get; set; }

	[JsonConstructor]
	protected GetChallengeStatusResponse()
	{
	}

	public GetChallengeStatusResponse(StatusEnum status = (StatusEnum)0, Guid sessionId = default(Guid), string approverEmail = null)
	{
		Status = status;
		SessionId = sessionId;
		ApproverEmail = approverEmail;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GetChallengeStatusResponse {\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  SessionId: ").Append(SessionId).Append("\n");
		stringBuilder.Append("  ApproverEmail: ").Append(ApproverEmail).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
