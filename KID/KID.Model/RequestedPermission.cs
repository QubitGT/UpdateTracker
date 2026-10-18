using System;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace KID.Model;

[DataContract(Name = "RequestedPermission")]
public class RequestedPermission
{
	[DataMember(Name = "name", IsRequired = true, EmitDefaultValue = true)]
	public string Name { get; set; }

	[JsonConstructor]
	protected RequestedPermission()
	{
	}

	public RequestedPermission(string name = null)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name is a required property for RequestedPermission and cannot be null");
		}
		Name = name;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("class RequestedPermission {\n");
		stringBuilder.Append("  Name: ").Append(Name).Append("\n");
		stringBuilder.Append("}\n");
		return stringBuilder.ToString();
	}

	public virtual string ToJson()
	{
		return JsonConvert.SerializeObject(this, Formatting.Indented);
	}
}
