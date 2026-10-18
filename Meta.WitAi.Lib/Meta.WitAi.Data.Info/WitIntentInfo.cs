using System;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitIntentInfo
{
	[SerializeField]
	public string id;

	[SerializeField]
	public string name;

	[SerializeField]
	public WitIntentEntityInfo[] entities;
}
