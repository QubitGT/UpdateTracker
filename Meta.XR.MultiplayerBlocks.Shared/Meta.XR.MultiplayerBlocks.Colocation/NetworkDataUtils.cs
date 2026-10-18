using System.Collections.Generic;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal static class NetworkDataUtils
{
	public static ulong? GetOculusIdOfColocatedGroupOwnerFromColocationGroupId(uint colocationGroupId)
	{
		foreach (Anchor allAnchor in NetworkAdapter.NetworkData.GetAllAnchors())
		{
			if (colocationGroupId == allAnchor.colocationGroupId)
			{
				return allAnchor.ownerOculusId;
			}
		}
		return null;
	}

	public static List<Player> GetAllPlayersFromColocationGroupId(uint colocationGroupId)
	{
		List<Player> allPlayers = NetworkAdapter.NetworkData.GetAllPlayers();
		List<Player> list = new List<Player>();
		foreach (Player item in allPlayers)
		{
			if (colocationGroupId == item.colocationGroupId)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<Player> GetAllPlayersColocatedWith(ulong oculusId, bool includeMyself)
	{
		INetworkData networkData = NetworkAdapter.NetworkData;
		List<Player> allPlayers = networkData.GetAllPlayers();
		uint colocationGroupId = networkData.GetPlayerWithOculusId(oculusId).Value.colocationGroupId;
		List<Player> list = new List<Player>();
		foreach (Player item in allPlayers)
		{
			if (item.colocationGroupId == colocationGroupId)
			{
				list.Add(item);
				if (!includeMyself && item.oculusId == oculusId)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
		}
		return list;
	}

	public static Player? GetPlayerFromOculusId(ulong oculusId)
	{
		return NetworkAdapter.NetworkData.GetPlayerWithOculusId(oculusId);
	}
}
