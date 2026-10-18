using UnityEngine;

namespace Fusion;

public interface INetworkTRSPTeleport
{
	void Teleport(Vector3? position = null, Quaternion? rotation = null);
}
