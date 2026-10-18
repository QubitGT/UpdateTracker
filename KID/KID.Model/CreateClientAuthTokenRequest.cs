using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "CreateClientAuthTokenRequest")]
public class CreateClientAuthTokenRequest
{
	[DataMember(Name = "clientId", IsRequired = true, EmitDefaultValue = true)]
	public string ClientId { get; set; }

	[JsonConstructor]
	protected CreateClientAuthTokenRequest()
	{
	}

	public CreateClientAuthTokenRequest(string clientId = null)
	{
		if (clientId == null)
		{
			throw new ArgumentNullException("clientId is a required property for CreateClientAuthTokenRequest and cannot be null");
		}
		ClientId = clientId;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class CreateClientAuthTokenRequest {\n");
		stringBuilder.Append("  ClientId: ").Append(ClientId).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
