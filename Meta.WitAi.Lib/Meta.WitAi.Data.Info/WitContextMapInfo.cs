using System;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitContextMapInfo
{
	[SerializeField]
	public string key;

	[SerializeField]
	public string value;
}
