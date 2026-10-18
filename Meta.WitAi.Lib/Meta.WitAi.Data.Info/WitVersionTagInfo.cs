using System;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitVersionTagInfo
{
	[SerializeField]
	public string name;

	[SerializeField]
	public string created_at;

	[SerializeField]
	public string updated_at;

	[SerializeField]
	public string desc;

	public WitVersionTagInfo(string name, string createdAt, string updatedAt, string description)
	{
		this.name = name;
		created_at = createdAt;
		updated_at = updatedAt;
		desc = description;
	}
}
