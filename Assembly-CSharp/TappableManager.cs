using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class TappableManager : NetworkSceneObject
{
	private static TappableManager gManager;

	[SerializeField]
	private List<Tappable> tappables = new List<Tappable>();

	private HashSet<int> idSet = new HashSet<int>();

	private static HashSet<Tappable> gRegistry = new HashSet<Tappable>();

	private void Awake()
	{
		if (gManager != null && gManager != this)
		{
			GTDev.LogWarning("Instance of TappableManager already exists. Destroying.");
			Object.Destroy(this);
			return;
		}
		if (gManager == null)
		{
			gManager = this;
		}
		if (gRegistry.Count == 0)
		{
			return;
		}
		Tappable[] array = gRegistry.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i] == null))
			{
				RegisterInstance(array[i]);
			}
		}
		gRegistry.Clear();
	}

	private void RegisterInstance(Tappable t)
	{
		if (t == null)
		{
			GTDev.LogError("Tappable is null.");
			return;
		}
		t.manager = this;
		if (idSet.Add(t.tappableId))
		{
			tappables.Add(t);
		}
	}

	private void UnregisterInstance(Tappable t)
	{
		if (!(t == null) && idSet.Remove(t.tappableId))
		{
			tappables.Remove(t);
			t.manager = null;
		}
	}

	public static void Register(Tappable t)
	{
		if (gManager != null)
		{
			gManager.RegisterInstance(t);
		}
		else
		{
			gRegistry.Add(t);
		}
	}

	public static void Unregister(Tappable t)
	{
		if (gManager != null)
		{
			gManager.UnregisterInstance(t);
		}
		else
		{
			gRegistry.Remove(t);
		}
	}

	[Conditional("QATESTING")]
	public void DebugTestTap()
	{
		if (tappables.Count > 0)
		{
			int index = Random.Range(0, tappables.Count);
			UnityEngine.Debug.Log("Send TestTap to tappable index: " + index + "/" + tappables.Count);
			tappables[index].OnTap(10f);
		}
		else
		{
			UnityEngine.Debug.Log("TappableManager: tappables array is empty.");
		}
	}

	[PunRPC]
	public void SendOnTapRPC(int key, float tapStrength, PhotonMessageInfo info)
	{
		SendOnTapShared(key, tapStrength, new PhotonMessageInfoWrapped(info));
	}

	private void SendOnTapShared(int key, float tapStrength, PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "SendOnTapShared");
		if (key == 0 || !float.IsFinite(tapStrength))
		{
			return;
		}
		tapStrength = Mathf.Clamp(tapStrength, 0f, 1f);
		for (int i = 0; i < tappables.Count; i++)
		{
			Tappable tappable = tappables[i];
			if (tappable.tappableId == key && !tappable.IsLocalOnly)
			{
				tappable.OnTapLocal(tapStrength, Time.time, info);
			}
		}
	}

	[PunRPC]
	public void SendOnGrabRPC(int key, PhotonMessageInfo info)
	{
		SendOnGrabShared(key, new PhotonMessageInfoWrapped(info));
	}

	private void SendOnGrabShared(int key, PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "SendOnGrabShared");
		if (key == 0)
		{
			return;
		}
		for (int i = 0; i < tappables.Count; i++)
		{
			Tappable tappable = tappables[i];
			if (tappable.tappableId == key && !tappable.IsLocalOnly)
			{
				tappable.OnGrabLocal(Time.time, info);
			}
		}
	}

	[PunRPC]
	public void SendOnReleaseRPC(int key, PhotonMessageInfo info)
	{
		SendOnReleaseShared(key, new PhotonMessageInfoWrapped(info));
	}

	public void SendOnReleaseShared(int key, PhotonMessageInfoWrapped info)
	{
		MonkeAgent.IncrementRPCCall(info, "SendOnReleaseShared");
		if (key == 0)
		{
			return;
		}
		for (int i = 0; i < tappables.Count; i++)
		{
			Tappable tappable = tappables[i];
			if (tappable.tappableId == key)
			{
				tappable.OnReleaseLocal(Time.time, info);
			}
		}
	}
}
