using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using KID.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "Session")]
public class Session
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum StatusEnum
	{
		[EnumMember(Value = "ACTIVE")]
		ACTIVE = 1,
		[EnumMember(Value = "HOLD")]
		HOLD
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ManagedByEnum
	{
		[EnumMember(Value = "PLAYER")]
		PLAYER = 1,
		[EnumMember(Value = "GUARDIAN")]
		GUARDIAN
	}

	[DataMember(Name = "status", IsRequired = true, EmitDefaultValue = true)]
	public StatusEnum Status { get; set; }

	[DataMember(Name = "ageStatus", IsRequired = true, EmitDefaultValue = true)]
	public AgeStatusType AgeStatus { get; set; }

	[DataMember(Name = "ageCategory", IsRequired = true, EmitDefaultValue = true)]
	public AgeCategoryV2 AgeCategory { get; set; }

	[DataMember(Name = "managedBy", IsRequired = true, EmitDefaultValue = true)]
	public ManagedByEnum ManagedBy { get; set; }

	[DataMember(Name = "sessionId", IsRequired = true, EmitDefaultValue = true)]
	public Guid SessionId { get; set; }

	[DataMember(Name = "kuid", EmitDefaultValue = false)]
	public string Kuid { get; set; }

	[DataMember(Name = "etag", IsRequired = true, EmitDefaultValue = true)]
	public string Etag { get; set; }

	[DataMember(Name = "permissions", EmitDefaultValue = false)]
	public List<Permission> Permissions { get; set; }

	[DataMember(Name = "dateOfBirth", IsRequired = true, EmitDefaultValue = true)]
	[JsonConverter(typeof(OpenAPIDateConverter))]
	public DateTime DateOfBirth { get; set; }

	[DataMember(Name = "jurisdiction", IsRequired = true, EmitDefaultValue = true)]
	public string Jurisdiction { get; set; }

	[JsonConstructor]
	protected Session()
	{
	}

	public Session(Guid sessionId = default(Guid), string kuid = null, string etag = null, StatusEnum status = (StatusEnum)0, List<Permission> permissions = null, AgeStatusType ageStatus = (AgeStatusType)0, AgeCategoryV2 ageCategory = (AgeCategoryV2)0, DateTime dateOfBirth = default(DateTime), string jurisdiction = null, ManagedByEnum managedBy = (ManagedByEnum)0)
	{
		SessionId = sessionId;
		if (etag == null)
		{
			throw new ArgumentNullException("etag is a required property for Session and cannot be null");
		}
		Etag = etag;
		Status = status;
		AgeStatus = ageStatus;
		AgeCategory = ageCategory;
		DateOfBirth = dateOfBirth;
		if (jurisdiction == null)
		{
			throw new ArgumentNullException("jurisdiction is a required property for Session and cannot be null");
		}
		Jurisdiction = jurisdiction;
		ManagedBy = managedBy;
		Kuid = kuid;
		Permissions = permissions;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class Session {\n");
		stringBuilder.Append("  SessionId: ").Append(SessionId).Append("\n");
		stringBuilder.Append("  Kuid: ").Append(Kuid).Append("\n");
		stringBuilder.Append("  Etag: ").Append(Etag).Append("\n");
		stringBuilder.Append("  Status: ").Append(Status).Append("\n");
		stringBuilder.Append("  Permissions: ").Append(Permissions).Append("\n");
		stringBuilder.Append("  AgeStatus: ").Append(AgeStatus).Append("\n");
		stringBuilder.Append("  AgeCategory: ").Append(AgeCategory).Append("\n");
		stringBuilder.Append("  DateOfBirth: ").Append(DateOfBirth).Append("\n");
		stringBuilder.Append("  Jurisdiction: ").Append(Jurisdiction).Append("\n");
		stringBuilder.Append("  ManagedBy: ").Append(ManagedBy).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
