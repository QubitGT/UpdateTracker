using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "GetAdultVerificationRequestStatusResponse")]
public class GetAdultVerificationRequestStatusResponse
{
	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public VerificationStatus Status { get; set; }

	[DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
	public Guid Id { get; set; }

	[DataMember(Name = "ageRange", EmitDefaultValue = false)]
	public AgeRange AgeRange { get; set; }

	[JsonConstructor]
	protected GetAdultVerificationRequestStatusResponse()
	{
	}

	public GetAdultVerificationRequestStatusResponse(Guid id = default(Guid), VerificationStatus status = (VerificationStatus)0, AgeRange ageRange = null)
	{
		Id = id;
		Status = status;
		AgeRange = ageRange;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GetAdultVerificationRequestStatusResponse {\n");
		stringBuilder.Append("  Id: ").Append(Id).Append("\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  AgeRange: ").Append(AgeRange).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
