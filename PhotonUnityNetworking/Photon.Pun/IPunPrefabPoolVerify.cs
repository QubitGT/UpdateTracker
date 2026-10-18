using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun;

public interface IPunPrefabPoolVerify : IPunPrefabPool
{
	bool VerifyInstantiation(Player sender, string prefabId, Vector3 position, Quaternion rotation, int[] viewIds, out GameObject prefab);

	GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation);
}
