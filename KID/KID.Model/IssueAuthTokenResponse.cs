using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "IssueAuthTokenResponse")]
public class IssueAuthTokenResponse
{
	[DataMember(Name = "accessToken", EmitDefaultValue = false)]
	public string AccessToken { get; set; }

	[DataMember(Name = "refreshToken", EmitDefaultValue = false)]
	public string RefreshToken { get; set; }

	public IssueAuthTokenResponse(string accessToken = null, string refreshToken = null)
	{
		AccessToken = accessToken;
		RefreshToken = refreshToken;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class IssueAuthTokenResponse {\n");
		stringBuilder.Append("  AccessToken: ").Append(AccessToken).Append("\n");
		stringBuilder.Append("  RefreshToken: ").Append(RefreshToken).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
