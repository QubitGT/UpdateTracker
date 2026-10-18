using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "SetChallengeStatusRequest")]
public class SetChallengeStatusRequest
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum StatusEnum
	{
		[EnumMember(Value = "PASS")]
		PASS = 1,
		[EnumMember(Value = "FAIL")]
		FAIL
	}

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public StatusEnum Status { get; set; }

	[DataMember(Name = "challengeId", IsRequired = true, EmitDefaultValue = true)]
	public Guid ChallengeId { get; set; }

	[DataMember(Name = "age", IsRequired = true, EmitDefaultValue = true)]
	public int Age { get; set; }

	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[DataMember(Name = "approverEmail", EmitDefaultValue = false)]
	public string ApproverEmail { get; set; }

	[JsonConstructor]
	protected SetChallengeStatusRequest()
	{
	}

	public SetChallengeStatusRequest(Guid challengeId = default(Guid), StatusEnum status = (StatusEnum)0, int age = 0, string jurisdiction = null, string approverEmail = null)
	{
		ChallengeId = challengeId;
		Status = status;
		Age = age;
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for SetChallengeStatusRequest and cannot be null");
		}
		Jurisdiction = jurisdiction;
		ApproverEmail = approverEmail;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class SetChallengeStatusRequest {\n");
		stringBuilder.Append("  ChallengeId: ").Append(ChallengeId).Append("\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  Age: ").Append(Age).Append("\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  ApproverEmail: ").Append(ApproverEmail).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
