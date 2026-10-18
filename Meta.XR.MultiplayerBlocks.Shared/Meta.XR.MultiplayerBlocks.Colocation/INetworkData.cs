using System.Collections.Generic;

namespace Meta.XR.MultiplayerBlocks.Colocation;

internal interface INetworkData
{
	void AddPlayer(Player player);

	void RemovePlayer(Player player);

	Player? GetPlayerWithPlayerId(ulong playerId);

	Player? GetPlayerWithOculusId(ulong oculusId);

	List<Player> GetAllPlayers();

	void AddAnchor(Anchor anchor);

	void RemoveAnchor(Anchor anchor);

	Anchor? GetAnchor(ulong ownerOculusId);

	List<Anchor> GetAllAnchors();

	uint GetColocationGroupCount();

	void IncrementColocationGroupCount();
}
