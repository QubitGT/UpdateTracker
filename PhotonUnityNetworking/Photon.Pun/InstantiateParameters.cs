using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun;

public struct InstantiateParameters
{
	public int[] viewIDs;

	public byte objLevelPrefix;

	public object[] data;

	public byte group;

	public Quaternion rotation;

	public Vector3 position;

	public string prefabName;

	public Player creator;

	public int timestamp;

	public InstantiateParameters(string prefabName, Vector3 position, Quaternion rotation, byte group, object[] data, byte objLevelPrefix, int[] viewIDs, Player creator, int timestamp)
	{
		this.prefabName = prefabName;
		this.position = position;
		this.rotation = rotation;
		this.group = group;
		this.data = data;
		this.objLevelPrefix = objLevelPrefix;
		this.viewIDs = viewIDs;
		this.creator = creator;
		this.timestamp = timestamp;
	}
}
