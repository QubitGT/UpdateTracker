using System;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public class WitTraitInfo
{
	[SerializeField]
	public string name;

	[SerializeField]
	public string id;

	[SerializeField]
	public WitTraitValueInfo[] values;
}
