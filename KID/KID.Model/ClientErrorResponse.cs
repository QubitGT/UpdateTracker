using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "ClientErrorResponse")]
public class ClientErrorResponse
{
	[DataMember(Name = "error", IsRequired = true, EmitDefaultValue = true)]
	public string Error { get; set; }

	[DataMember(Name = "errorMessage", EmitDefaultValue = false)]
	public string ErrorMessage { get; set; }

	[JsonConstructor]
	protected ClientErrorResponse()
	{
	}

	public ClientErrorResponse(string error = null, string errorMessage = null)
	{
		if (error == null)
		{
			throw new ArgumentNullException("error is a required property for ClientErrorResponse and cannot be null");
		}
		Error = error;
		ErrorMessage = errorMessage;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class ClientErrorResponse {\n");
		stringBuilder.Append("  Error: ").Append(Error).Append("\n");
		stringBuilder.Append("  ErrorMessage: ").Append(ErrorMessage).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
