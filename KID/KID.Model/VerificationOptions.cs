using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "VerificationOptions")]
public class VerificationOptions
{
	[DataMember(Name = "sendEmail", EmitDefaultValue = true)]
	public bool SendEmail { get; set; }

	public VerificationOptions(bool sendEmail = false)
	{
		SendEmail = sendEmail;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class VerificationOptions {\n");
		stringBuilder.Append("  SendEmail: ").Append(SendEmail).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
