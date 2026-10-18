using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateAgeAssuranceVerificationResponse")]
public class CreateAgeAssuranceVerificationResponse
{
	[DataMember(Name = "id", IsRequired = true, EmitDefaultValue = true)]
	public Guid Id { get; set; }

	[DataMember(Name = "url", IsRequired = true, EmitDefaultValue = true)]
	public string Url { get; set; }

	[JsonConstructor]
	protected CreateAgeAssuranceVerificationResponse()
	{
	}

	public CreateAgeAssuranceVerificationResponse(Guid id = default(Guid), string url = null)
	{
		Id = id;
		if (url == null)
		{
			throw new ArgumentNullException("url is a required property for CreateAgeAssuranceVerificationResponse and cannot be null");
		}
		Url = url;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateAgeAssuranceVerificationResponse {\n");
		stringBuilder.Append("  Id: ").Append(Id).Append("\n");
		stringBuilder.Append("  Url: ").Append(Url).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
