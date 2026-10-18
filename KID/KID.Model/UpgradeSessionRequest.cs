using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "UpgradeSessionRequest")]
public class UpgradeSessionRequest
{
	[DataMember(Name = "sessionId", IsRequired = true, EmitDefaultValue = true)]
	public Guid SessionId { get; set; }

	[DataMember(Name = "requestedPermissions", IsRequired = true, EmitDefaultValue = true)]
	public List<RequestedPermission> RequestedPermissions { get; set; }

	[JsonConstructor]
	protected UpgradeSessionRequest()
	{
	}

	public UpgradeSessionRequest(Guid sessionId = default(Guid), List<RequestedPermission> requestedPermissions = null)
	{
		SessionId = sessionId;
		if (requestedPermissions == null)
		{
			throw new ArgumentNullException("requestedPermissions is a required property for UpgradeSessionRequest and cannot be null");
		}
		RequestedPermissions = requestedPermissions;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class UpgradeSessionRequest {\n");
		stringBuilder.Append("  SessionId: ").Append(SessionId).Append("\n");
		stringBuilder.Append("  RequestedPermissions: ").Append(RequestedPermissions).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
