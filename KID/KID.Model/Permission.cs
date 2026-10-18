using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KID.Model;

[DataContract(Name = "Permission")]
public class Permission
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ManagedByEnum
	{
		[EnumMember(Value = "PLAYER")]
		PLAYER = 1,
		[EnumMember(Value = "GUARDIAN")]
		GUARDIAN,
		[EnumMember(Value = "PROHIBITED")]
		PROHIBITED
	}

	[DataMember(Name = "managedBy", IsRequired = true, EmitDefaultValue = true)]
	public ManagedByEnum ManagedBy { get; set; }

	[DataMember(Name = "name", IsRequired = true, EmitDefaultValue = true)]
	public string Name { get; set; }

	[DataMember(Name = "enabled", IsRequired = true, EmitDefaultValue = true)]
	public bool Enabled { get; set; }

	[JsonConstructor]
	protected Permission()
	{
	}

	public Permission(string name = null, bool enabled = false, ManagedByEnum managedBy = (ManagedByEnum)0)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name is a required property for Permission and cannot be null");
		}
		Name = name;
		Enabled = enabled;
		ManagedBy = managedBy;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class Permission {\n");
		stringBuilder.Append("  Name: ").Append(Name).Append("\n");
		stringBuilder.Append("  Enabled: ").Append(Enabled).Append("\n");
		stringBuilder.Append("  ManagedBy: ").Append(ManagedBy).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
