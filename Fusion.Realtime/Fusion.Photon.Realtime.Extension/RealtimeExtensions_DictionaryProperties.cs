using System.Collections.Generic;

namespace Fusion.Photon.Realtime.Extension;

internal static class RealtimeExtensions_DictionaryProperties
{
	public static int CalculateTotalSize(Dictionary<string, SessionProperty> dictionary)
	{
		return dictionary.ConvertToHashtable().CalculateTotalSize();
	}
}
