using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "refreshClientAuthToken_request")]
public class RefreshClientAuthTokenRequest
{
	[DataMember(Name = "refreshToken", IsRequired = true, EmitDefaultValue = true)]
	public string RefreshToken { get; set; }

	[JsonConstructor]
	protected RefreshClientAuthTokenRequest()
	{
	}

	public RefreshClientAuthTokenRequest(string refreshToken = null)
	{
		if (refreshToken == null)
		{
			throw new ArgumentNullException("refreshToken is a required property for RefreshClientAuthTokenRequest and cannot be null");
		}
		RefreshToken = refreshToken;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class RefreshClientAuthTokenRequest {\n");
		stringBuilder.Append("  RefreshToken: ").Append(RefreshToken).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
