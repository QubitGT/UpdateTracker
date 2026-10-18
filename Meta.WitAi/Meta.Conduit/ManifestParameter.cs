using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.Conduit;

internal class ManifestParameter
{
	private string _name;

	[Preserve]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = ConduitUtilities.DelimitWithUnderscores(value);
		}
	}

	[Preserve]
	public string InternalName { get; set; }

	[Preserve]
	public string QualifiedName { get; set; }

	[JsonIgnore]
	public string EntityType
	{
		get
		{
			int num = QualifiedTypeName.LastIndexOf('.');
			if (num < 0)
			{
				return QualifiedTypeName;
			}
			string text = QualifiedTypeName.Substring(num + 1);
			int num2 = text.LastIndexOf('+');
			if (num2 < 0)
			{
				return text;
			}
			return text.Substring(num2 + 1);
		}
	}

	[Preserve]
	public string TypeAssembly { get; set; }

	[Preserve]
	public string QualifiedTypeName { get; set; }

	[Preserve]
	public List<string> Aliases { get; set; }

	[Preserve]
	public List<string> Examples { get; set; }

	[Preserve]
	public ManifestParameter()
	{
	}

	public override bool Equals(object obj)
	{
		if (obj is ManifestParameter other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((17 * 31 + _name.GetHashCode()) * 31 + InternalName.GetHashCode()) * 31 + QualifiedName.GetHashCode()) * 31 + TypeAssembly.GetHashCode()) * 31 + QualifiedTypeName.GetHashCode()) * 31 + Aliases.GetHashCode();
	}

	private bool Equals(ManifestParameter other)
	{
		if (object.Equals(InternalName, other.InternalName) && object.Equals(QualifiedName, other.QualifiedName) && object.Equals(EntityType, other.EntityType) && Aliases.SequenceEqual(other.Aliases) && object.Equals(TypeAssembly, other.TypeAssembly))
		{
			return object.Equals(QualifiedTypeName, other.QualifiedTypeName);
		}
		return false;
	}
}
