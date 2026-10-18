using System;
using UnityEngine;

namespace Meta.WitAi.Composer.Data.Info;

[Serializable]
public struct ComposerGraphValues
{
	[Tooltip("The path name referenced in Composer")]
	public string path;

	[Tooltip("The values assigned to this path in Composer")]
	public string[] values;
}
