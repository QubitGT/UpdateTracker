using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Data.Info;
using UnityEngine.Scripting;

namespace Meta.Conduit;

internal class ManifestEntity
{
	[Preserve]
	public string ID { get; set; }

	[Preserve]
	public string Namespace { get; set; }

	[Preserve]
	public string Type { get; set; }

	[Preserve]
	public string Name { get; set; }

	[Preserve]
	public List<WitKeyword> Values { get; set; } = new List<WitKeyword>();


	[Preserve]
	public string Assembly { get; set; }

	[Preserve]
	public ManifestEntity()
	{
	}

	public WitEntityInfo GetAsInfo()
	{
		WitEntityKeywordInfo[] array = new WitEntityKeywordInfo[Values.Count];
		for (int i = 0; i < Values.Count; i++)
		{
			array[i] = Values[i].GetAsInfo();
		}
		WitEntityInfo result = default(WitEntityInfo);
		result.name = Name;
		result.keywords = array;
		result.roles = new WitEntityRoleInfo[0];
		return result;
	}

	public string GetQualifiedTypeName()
	{
		if (!string.IsNullOrEmpty(Namespace))
		{
			return Namespace + "." + ID;
		}
		return ID ?? "";
	}

	public override bool Equals(object obj)
	{
		if (obj is ManifestEntity other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((((17 * 31 + ID.GetHashCode()) * 31 + Type.GetHashCode()) * 31 + Name.GetHashCode()) * 31 + Values.GetHashCode()) * 31 + Namespace.GetHashCode()) * 31 + Assembly.GetHashCode();
	}

	private bool Equals(ManifestEntity other)
	{
		if (ID == other.ID && Type == other.Type && Name == other.Name && Namespace == other.Namespace && Assembly == other.Assembly)
		{
			return Values.SequenceEqual(other.Values);
		}
		return false;
	}
}
