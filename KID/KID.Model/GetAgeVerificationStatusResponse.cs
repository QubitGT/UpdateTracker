using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "GetAgeVerificationStatusResponse")]
public class GetAgeVerificationStatusResponse
{
	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public VerificationStatusV2 Status { get; set; }

	[DataMember(Name = "ageCategory", EmitDefaultValue = false)]
	public AgeCategoryV2? AgeCategory { get; set; }

	[DataMember(Name = "method", EmitDefaultValue = false)]
	public VerificationMethod? Method { get; set; }

	[DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
	public Guid Id { get; set; }

	[DataMember(Name = "age", EmitDefaultValue = false)]
	public AgeRangeV2 Age { get; set; }

	[JsonConstructor]
	protected GetAgeVerificationStatusResponse()
	{
	}

	public GetAgeVerificationStatusResponse(Guid id = default(Guid), VerificationStatusV2 status = (VerificationStatusV2)0, AgeRangeV2 age = null, AgeCategoryV2? ageCategory = null, VerificationMethod? method = null)
	{
		Id = id;
		Status = status;
		Age = age;
		AgeCategory = ageCategory;
		Method = method;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GetAgeVerificationStatusResponse {\n");
		stringBuilder.Append("  Id: ").Append(Id).Append("\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  Age: ").Append(Age).Append("\n");
		stringBuilder.Append("  AgeCategory: ").Append(AgeCategory).Append("\n");
		stringBuilder.Append("  Method: ").Append(Method).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
