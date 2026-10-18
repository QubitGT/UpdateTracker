using System;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitEntityInfo
{
	[SerializeField]
	public string name;

	[SerializeField]
	public string id;

	[SerializeField]
	public string[] lookups;

	[SerializeField]
	public WitEntityRoleInfo[] roles;

	[SerializeField]
	public WitEntityKeywordInfo[] keywords;

	public override bool Equals(object obj)
	{
		if (obj is WitEntityInfo other)
		{
			return Equals(other);
		}
		return false;
	}

	public bool Equals(WitEntityInfo other)
	{
		if (name == other.name && id == other.id && lookups.Equivalent(other.lookups) && roles.Equivalent(other.roles))
		{
			return keywords.Equivalent(other.keywords);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((17 * 31 + name.GetHashCode()) * 31 + id.GetHashCode()) * 31 + lookups.GetHashCode()) * 31 + roles.GetHashCode()) * 31 + keywords.GetHashCode();
	}
}
