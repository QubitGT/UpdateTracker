using System;
using Modio.Mods;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertySubscribers : ModPropertyNumberBase
{
	protected override long GetValue(Mod mod)
	{
		return mod.Stats.Subscribers;
	}
}
