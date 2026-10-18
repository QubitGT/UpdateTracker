using System;
using Meta.WitAi.Data.Configuration;
using UnityEngine;

namespace Meta.WitAi.Composer.Data.Info;

[Serializable]
public class WitComposerData : WitConfigurationAssetData
{
	[Tooltip("Represents the canvas of the given name.")]
	public ComposerGraph[] canvases;
}
