using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "SetGuardianManagedSessionPermissionsResponse")]
public class SetGuardianManagedSessionPermissionsResponse
{
	[DataMember(Name = "sessionId", IsRequired = true, EmitDefaultValue = true)]
	public Guid SessionId { get; set; }

	[DataMember(Name = "enabledPermissions", IsRequired = true, EmitDefaultValue = true)]
	public List<string> EnabledPermissions { get; set; }

	[JsonConstructor]
	protected SetGuardianManagedSessionPermissionsResponse()
	{
	}

	public SetGuardianManagedSessionPermissionsResponse(Guid sessionId = default(Guid), List<string> enabledPermissions = null)
	{
		SessionId = sessionId;
		if (enabledPermissions == null)
		{
			throw new ArgumentNullException("enabledPermissions is a required property for SetGuardianManagedSessionPermissionsResponse and cannot be null");
		}
		EnabledPermissions = enabledPermissions;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class SetGuardianManagedSessionPermissionsResponse {\n");
		stringBuilder.Append("  SessionId: ").Append(SessionId).Append("\n");
		stringBuilder.Append("  EnabledPermissions: ").Append(EnabledPermissions).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
