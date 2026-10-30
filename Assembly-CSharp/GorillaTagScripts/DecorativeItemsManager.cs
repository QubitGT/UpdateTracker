using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts;

public class DecorativeItemsManager : NetworkComponent
{
	public GameObject decorativeItemsContainer;

	public GameObject respawnableHooksContainer;

	public List<GameObject> nonRespawnableHooksContainer = new List<GameObject>();

	private readonly List<DecorativeItem> itemsList = new List<DecorativeItem>();

	private readonly List<AttachPoint> respawnableHooks = new List<AttachPoint>();

	private readonly List<AttachPoint> allHooks = new List<AttachPoint>();

	private int lastIndex;

	private int currentIndex;

	private int arrayIndex = -1;

	private bool shouldRunUpdate;

	private ZoneBasedObject zone;

	private bool wasInZone;

	[OnEnterPlay_SetNull]
	private static DecorativeItemsManager _instance;

	public static DecorativeItemsManager Instance => _instance;

	protected override void Awake()
	{
		base.Awake();
		if (_instance != null && _instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
		currentIndex = -1;
		shouldRunUpdate = true;
		zone = GetComponent<ZoneBasedObject>();
		DecorativeItem[] componentsInChildren = decorativeItemsContainer.GetComponentsInChildren<DecorativeItem>(includeInactive: false);
		foreach (DecorativeItem decorativeItem in componentsInChildren)
		{
			if ((bool)decorativeItem)
			{
				itemsList.Add(decorativeItem);
				decorativeItem.respawnItem = (UnityAction<DecorativeItem>)Delegate.Combine(decorativeItem.respawnItem, new UnityAction<DecorativeItem>(OnRequestToRespawn));
			}
		}
		AttachPoint[] componentsInChildren2 = respawnableHooksContainer.GetComponentsInChildren<AttachPoint>(includeInactive: false);
		foreach (AttachPoint attachPoint in componentsInChildren2)
		{
			if ((bool)attachPoint)
			{
				respawnableHooks.Add(attachPoint);
			}
		}
		allHooks.AddRange(respawnableHooks);
		foreach (GameObject item in nonRespawnableHooksContainer)
		{
			componentsInChildren2 = item.GetComponentsInChildren<AttachPoint>(includeInactive: false);
			foreach (AttachPoint attachPoint2 in componentsInChildren2)
			{
				if ((bool)attachPoint2)
				{
					allHooks.Add(attachPoint2);
				}
			}
		}
	}

	private void OnDestroy()
	{
		foreach (DecorativeItem items in itemsList)
		{
			items.respawnItem = (UnityAction<DecorativeItem>)Delegate.Remove(items.respawnItem, new UnityAction<DecorativeItem>(OnRequestToRespawn));
		}
		itemsList.Clear();
		respawnableHooks.Clear();
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void Update()
	{
		if (!PhotonNetwork.InRoom)
		{
			return;
		}
		if (wasInZone != zone.IsLocalPlayerInZone())
		{
			shouldRunUpdate = true;
		}
		if (!shouldRunUpdate || !base.IsMine)
		{
			return;
		}
		if (wasInZone != zone.IsLocalPlayerInZone())
		{
			foreach (AttachPoint allHook in allHooks)
			{
				allHook.SetIsHook(isHooked: false);
			}
			for (int i = 0; i < itemsList.Count; i++)
			{
				itemsList[i].itemState = TransferrableObject.ItemStates.State2;
				SpawnItem(i);
			}
			shouldRunUpdate = false;
		}
		wasInZone = zone.IsLocalPlayerInZone();
		SpawnItem(UpdateListPerFrame());
	}

	private void SpawnItem(int index)
	{
		if (!NetworkSystem.Instance.InRoom || index < 0 || index >= itemsList.Count || respawnableHooks == null || itemsList == null)
		{
			return;
		}
		if (itemsList.Count > respawnableHooks.Count)
		{
			Debug.LogError("Trying to snap more decorative items than allowed! Some items will be left un-hooked!");
			return;
		}
		Transform transform = RandomSpawn();
		if (!(transform == null))
		{
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			DecorativeItem decorativeItem = itemsList[index];
			decorativeItem.WorldShareableRequestOwnership();
			decorativeItem.Respawn(position, rotation);
			SendRPC("RespawnItemRPC", RpcTarget.Others, index, position, rotation);
		}
	}

	[PunRPC]
	private void RespawnItemRPC(int index, Vector3 _transformPos, Quaternion _transformRot, PhotonMessageInfo info)
	{
		RespawnItemShared(index, _transformPos, _transformRot, info);
	}

	protected void RespawnItemShared(int index, Vector3 _transformPos, Quaternion _transformRot, PhotonMessageInfoWrapped info)
	{
		if (index >= 0 && index <= itemsList.Count - 1)
		{
			float maxVal = 10000f;
			if (_transformPos.IsValid(in maxVal) && _transformRot.IsValid() && info.Sender == NetworkSystem.Instance.MasterClient)
			{
				MonkeAgent.IncrementRPCCall(info, "RespawnItemRPC");
				itemsList[index].Respawn(_transformPos, _transformRot);
			}
		}
	}

	private Transform RandomSpawn()
	{
		lastIndex = currentIndex;
		bool flag = false;
		bool flag2 = zone.IsLocalPlayerInZone();
		int index = UnityEngine.Random.Range(0, respawnableHooks.Count);
		while (!flag)
		{
			index = UnityEngine.Random.Range(0, respawnableHooks.Count);
			if (!respawnableHooks[index].inForest == flag2)
			{
				flag = true;
			}
		}
		if (!respawnableHooks[index].IsHooked())
		{
			currentIndex = index;
		}
		else
		{
			currentIndex = -1;
		}
		if (currentIndex != lastIndex && currentIndex > -1)
		{
			return respawnableHooks[currentIndex].attachPoint;
		}
		currentIndex = -1;
		return null;
	}

	private int UpdateListPerFrame()
	{
		arrayIndex++;
		if (arrayIndex >= itemsList.Count || arrayIndex < 0)
		{
			shouldRunUpdate = false;
			return -1;
		}
		return arrayIndex;
	}

	private void OnRequestToRespawn(DecorativeItem item)
	{
		if (base.IsMine && !(item == null))
		{
			int index = itemsList.IndexOf(item);
			SpawnItem(index);
		}
	}

	public AttachPoint getCurrentAttachPointByPosition(Vector3 _attachPoint)
	{
		foreach (AttachPoint allHook in allHooks)
		{
			if (allHook.attachPoint.position == _attachPoint)
			{
				return allHook;
			}
		}
		return null;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			stream.SendNext(currentIndex);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			currentIndex = (int)stream.ReceiveNext();
		}
	}
}
