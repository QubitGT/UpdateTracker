using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "GetDefaultPermissionsResponse")]
public class GetDefaultPermissionsResponse
{
	[DataMember(Name = "ageStatus", IsRequired = true, EmitDefaultValue = true)]
	public AgeStatusType AgeStatus { get; set; }

	[DataMember(Name = "ageCategory", IsRequired = true, EmitDefaultValue = true)]
	public AgeCategoryV2 AgeCategory { get; set; }

	[DataMember(Name = "requiresParentConsentForDataProcessing", IsRequired = true, EmitDefaultValue = true)]
	public bool RequiresParentConsentForDataProcessing { get; set; }

	[DataMember(Name = "permissions", IsRequired = true, EmitDefaultValue = true)]
	public List<Permission> Permissions { get; set; }

	[JsonConstructor]
	protected GetDefaultPermissionsResponse()
	{
	}

	public GetDefaultPermissionsResponse(bool requiresParentConsentForDataProcessing = false, List<Permission> permissions = null, AgeStatusType ageStatus = (AgeStatusType)0, AgeCategoryV2 ageCategory = (AgeCategoryV2)0)
	{
		RequiresParentConsentForDataProcessing = requiresParentConsentForDataProcessing;
		if (permissions == null)
		{
			throw new ArgumentNullException("permissions is a required property for GetDefaultPermissionsResponse and cannot be null");
		}
		Permissions = permissions;
		AgeStatus = ageStatus;
		AgeCategory = ageCategory;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class GetDefaultPermissionsResponse {\n");
		stringBuilder.Append("  RequiresParentConsentForDataProcessing: ").Append(RequiresParentConsentForDataProcessing).Append("\n");
		stringBuilder.Append("  Permissions: ").Append(Permissions).Append("\n");
		stringBuilder.Append("  AgeStatus: ").Append(AgeStatus).Append("\n");
		stringBuilder.Append("  AgeCategory: ").Append(AgeCategory).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
