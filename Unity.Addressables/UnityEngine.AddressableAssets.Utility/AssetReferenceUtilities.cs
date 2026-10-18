using System;

namespace UnityEngine.AddressableAssets.Utility;

internal class AssetReferenceUtilities
{
	internal static string FormatName(string name)
	{
		if (name.EndsWith("(Clone)", StringComparison.Ordinal))
		{
			name = name.Replace("(Clone)", "");
		}
		return name;
	}
}
