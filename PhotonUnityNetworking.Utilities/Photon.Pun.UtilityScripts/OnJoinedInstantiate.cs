using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Pun.UtilityScripts;

public class OnJoinedInstantiate : MonoBehaviour, IMatchmakingCallbacks
{
	public enum SpawnSequence
	{
		Connection,
		Random,
		RoundRobin
	}

	[HideInInspector]
	private Transform SpawnPosition;

	[HideInInspector]
	public SpawnSequence Sequence;

	[HideInInspector]
	public List<Transform> SpawnPoints = new List<Transform>(1) { null };

	[Tooltip("Add a random variance to a spawn point position. GetRandomOffset() can be overridden with your own method for producing offsets.")]
	[HideInInspector]
	public bool UseRandomOffset = true;

	[Tooltip("Radius of the RandomOffset.")]
	[FormerlySerializedAs("PositionOffset")]
	[HideInInspector]
	public float RandomOffset = 2f;

	[Tooltip("Disables the Y axis of RandomOffset. The Y value of the spawn point will be used.")]
	[HideInInspector]
	public bool ClampY = true;

	[HideInInspector]
	public List<GameObject> PrefabsToInstantiate = new List<GameObject>(1) { null };

	[FormerlySerializedAs("autoSpawnObjects")]
	[HideInInspector]
	public bool AutoSpawnObjects = true;

	public Stack<GameObject> SpawnedObjects = new Stack<GameObject>();

	protected int spawnedAsActorId;

	protected int lastUsedSpawnPointIndex = -1;

	public virtual void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	public virtual void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	public virtual void OnJoinedRoom()
	{
		if (AutoSpawnObjects && !PhotonNetwork.LocalPlayer.HasRejoined)
		{
			SpawnObjects();
		}
	}

	public virtual void SpawnObjects()
	{
		if (PrefabsToInstantiate == null)
		{
			return;
		}
		foreach (GameObject item2 in PrefabsToInstantiate)
		{
			if (!(item2 == null))
			{
				GetSpawnPoint(out var spawnPos, out var spawnRot);
				GameObject item = PhotonNetwork.Instantiate(item2.name, spawnPos, spawnRot, 0);
				SpawnedObjects.Push(item);
			}
		}
	}

	public virtual void DespawnObjects(bool localOnly)
	{
		while (SpawnedObjects.Count > 0)
		{
			GameObject gameObject = SpawnedObjects.Pop();
			if ((bool)gameObject)
			{
				if (localOnly)
				{
					Object.Destroy(gameObject);
				}
				else
				{
					PhotonNetwork.Destroy(gameObject);
				}
			}
		}
	}

	public virtual void OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	public virtual void OnCreatedRoom()
	{
	}

	public virtual void OnCreateRoomFailed(short returnCode, string message)
	{
	}

	public virtual void OnJoinRoomFailed(short returnCode, string message)
	{
	}

	public virtual void OnJoinRandomFailed(short returnCode, string message)
	{
	}

	public virtual void OnLeftRoom()
	{
	}

	public virtual void OnPreLeavingRoom()
	{
	}

	public virtual void GetSpawnPoint(out Vector3 spawnPos, out Quaternion spawnRot)
	{
		Transform spawnPoint = GetSpawnPoint();
		if (spawnPoint != null)
		{
			spawnPos = spawnPoint.position;
			spawnRot = spawnPoint.rotation;
		}
		else
		{
			spawnPos = new Vector3(0f, 0f, 0f);
			spawnRot = new Quaternion(0f, 0f, 0f, 1f);
		}
		if (UseRandomOffset)
		{
			Random.InitState((int)(Time.time * 10000f));
			spawnPos += GetRandomOffset();
		}
	}

	protected virtual Transform GetSpawnPoint()
	{
		if (SpawnPoints == null || SpawnPoints.Count == 0)
		{
			return null;
		}
		switch (Sequence)
		{
		case SpawnSequence.Connection:
		{
			int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
			return SpawnPoints[(actorNumber != -1) ? (actorNumber % SpawnPoints.Count) : 0];
		}
		case SpawnSequence.RoundRobin:
			lastUsedSpawnPointIndex++;
			if (lastUsedSpawnPointIndex >= SpawnPoints.Count)
			{
				lastUsedSpawnPointIndex = 0;
			}
			if (SpawnPoints != null && SpawnPoints.Count != 0)
			{
				return SpawnPoints[lastUsedSpawnPointIndex];
			}
			return null;
		case SpawnSequence.Random:
			return SpawnPoints[Random.Range(0, SpawnPoints.Count)];
		default:
			return null;
		}
	}

	protected virtual Vector3 GetRandomOffset()
	{
		Vector3 insideUnitSphere = Random.insideUnitSphere;
		if (ClampY)
		{
			insideUnitSphere.y = 0f;
		}
		return RandomOffset * insideUnitSphere.normalized;
	}
}
