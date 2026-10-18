using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "TestVerificationWebhookRequest")]
public class TestVerificationWebhookRequest
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum EventTypeEnum
	{
		[EnumMember(Value = "adultVerificationResult")]
		AdultVerificationResult = 1,
		[EnumMember(Value = "ageAssuranceVerificationResult")]
		AgeAssuranceVerificationResult
	}

	[DataMember(Name = "eventType", EmitDefaultValue = false)]
	public EventTypeEnum? EventType { get; set; }

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public VerificationStatus Status { get; set; }

	[DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
	public Guid Id { get; set; }

	[DataMember(Name = "ageRange", EmitDefaultValue = false)]
	public AgeRange AgeRange { get; set; }

	[JsonConstructor]
	protected TestVerificationWebhookRequest()
	{
	}

	public TestVerificationWebhookRequest(EventTypeEnum? eventType = null, Guid id = default(Guid), AgeRange ageRange = null, VerificationStatus status = (VerificationStatus)0)
	{
		Id = id;
		Status = status;
		EventType = eventType;
		AgeRange = ageRange;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class TestVerificationWebhookRequest {\n");
		stringBuilder.Append("  EventType: ").Append(EventType).Append("\n");
		stringBuilder.Append("  Id: ").Append(Id).Append("\n");
		stringBuilder.Append("  AgeRange: ").Append(AgeRange).Append("\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
