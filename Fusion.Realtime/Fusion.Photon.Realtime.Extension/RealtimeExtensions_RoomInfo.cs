using System.Collections.Generic;

namespace Fusion.Photon.Realtime.Extension;

internal static class RealtimeExtensions_RoomInfo
{
	public static Dictionary<string, SessionProperty> GetCustomProperties(this RoomInfo roomInfo)
	{
		return roomInfo.CustomProperties.ConvertToDictionaryProperty();
	}
}
