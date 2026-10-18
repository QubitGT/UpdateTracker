using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "UpgradeSessionResponse")]
public class UpgradeSessionResponse
{
	[DataMember(Name = "session", IsRequired = true, EmitDefaultValue = true)]
	public Session Session { get; set; }

	[DataMember(Name = "challenge", EmitDefaultValue = false)]
	public Challenge Challenge { get; set; }

	[JsonConstructor]
	protected UpgradeSessionResponse()
	{
	}

	public UpgradeSessionResponse(Session session = null, Challenge challenge = null)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session is a required property for UpgradeSessionResponse and cannot be null");
		}
		Session = session;
		Challenge = challenge;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class UpgradeSessionResponse {\n");
		stringBuilder.Append("  Session: ").Append(Session).Append("\n");
		stringBuilder.Append("  Challenge: ").Append(Challenge).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
