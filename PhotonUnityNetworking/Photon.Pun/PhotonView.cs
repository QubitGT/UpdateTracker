using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Serialization;

namespace Photon.Pun;

[AddComponentMenu("Photon Networking/Photon View")]
public class PhotonView : MonoBehaviour
{
	public enum ObservableSearch
	{
		Manual,
		AutoFindActive,
		AutoFindAll
	}

	private struct CallbackTargetChange
	{
		public IPhotonViewCallback obj;

		public Type type;

		public bool add;

		public CallbackTargetChange(IPhotonViewCallback obj, Type type, bool add)
		{
			this.obj = obj;
			this.type = type;
			this.add = add;
		}
	}

	[FormerlySerializedAs("group")]
	public byte Group;

	[FormerlySerializedAs("prefixBackup")]
	public int prefixField = -1;

	internal object[] instantiationDataField;

	protected internal List<object> lastOnSerializeDataSent;

	protected internal List<object> syncValues;

	protected internal object[] lastOnSerializeDataReceived;

	[FormerlySerializedAs("synchronization")]
	public ViewSynchronization Synchronization = ViewSynchronization.UnreliableOnChange;

	protected internal bool mixedModeIsReliable;

	[FormerlySerializedAs("ownershipTransfer")]
	public OwnershipOption OwnershipTransfer;

	public ObservableSearch observableSearch;

	public List<Component> ObservedComponents;

	internal MonoBehaviour[] RpcMonoBehaviours;

	[NonSerialized]
	private int ownerActorNr;

	[NonSerialized]
	private int controllerActorNr;

	[SerializeField]
	[FormerlySerializedAs("viewIdField")]
	[HideInInspector]
	public int sceneViewId;

	[NonSerialized]
	private int viewIdField;

	[FormerlySerializedAs("instantiationId")]
	public int InstantiationId;

	[SerializeField]
	[HideInInspector]
	public bool isRuntimeInstantiated;

	protected internal bool removedFromLocalViewList;

	private Queue<CallbackTargetChange> CallbackChangeQueue = new Queue<CallbackTargetChange>();

	private List<IOnPhotonViewPreNetDestroy> OnPreNetDestroyCallbacks;

	private List<IOnPhotonViewOwnerChange> OnOwnerChangeCallbacks;

	private List<IOnPhotonViewControllerChange> OnControllerChangeCallbacks;

	public int Prefix
	{
		get
		{
			if (prefixField == -1 && PhotonNetwork.NetworkingClient != null)
			{
				prefixField = PhotonNetwork.currentLevelPrefix;
			}
			return prefixField;
		}
		set
		{
			prefixField = value;
		}
	}

	public object[] InstantiationData
	{
		get
		{
			return instantiationDataField;
		}
		protected internal set
		{
			instantiationDataField = value;
		}
	}

	[Obsolete("Renamed. Use IsRoomView instead")]
	public bool IsSceneView => IsRoomView;

	public bool IsRoomView => CreatorActorNr == 0;

	public bool IsOwnerActive
	{
		get
		{
			if (Owner != null)
			{
				return !Owner.IsInactive;
			}
			return false;
		}
	}

	public bool IsMine { get; private set; }

	public bool AmController => IsMine;

	public Player Controller { get; private set; }

	public int CreatorActorNr { get; private set; }

	public bool AmOwner { get; private set; }

	public Player Owner { get; private set; }

	public int OwnerActorNr
	{
		get
		{
			return ownerActorNr;
		}
		set
		{
			if (value != 0 && ownerActorNr == value)
			{
				return;
			}
			Player owner = Owner;
			Owner = ((PhotonNetwork.CurrentRoom == null) ? null : PhotonNetwork.CurrentRoom.GetPlayer(value, findMaster: true));
			ownerActorNr = ((Owner != null) ? Owner.ActorNumber : value);
			AmOwner = PhotonNetwork.LocalPlayer != null && ownerActorNr == PhotonNetwork.LocalPlayer.ActorNumber;
			UpdateCallbackLists();
			if (OnOwnerChangeCallbacks != null)
			{
				int i = 0;
				for (int count = OnOwnerChangeCallbacks.Count; i < count; i++)
				{
					OnOwnerChangeCallbacks[i].OnOwnerChange(Owner, owner);
				}
			}
		}
	}

	public int ControllerActorNr
	{
		get
		{
			return controllerActorNr;
		}
		set
		{
			Player controller = Controller;
			Controller = ((PhotonNetwork.CurrentRoom == null) ? null : PhotonNetwork.CurrentRoom.GetPlayer(value, findMaster: true));
			if (Controller != null && Controller.IsInactive)
			{
				Controller = PhotonNetwork.MasterClient;
			}
			controllerActorNr = ((Controller != null) ? Controller.ActorNumber : value);
			IsMine = PhotonNetwork.LocalPlayer != null && controllerActorNr == PhotonNetwork.LocalPlayer.ActorNumber;
			if (Controller == controller)
			{
				return;
			}
			UpdateCallbackLists();
			if (OnControllerChangeCallbacks != null)
			{
				int i = 0;
				for (int count = OnControllerChangeCallbacks.Count; i < count; i++)
				{
					OnControllerChangeCallbacks[i].OnControllerChange(Controller, controller);
				}
			}
		}
	}

	public int ViewID
	{
		get
		{
			return viewIdField;
		}
		set
		{
			if (value != 0 && viewIdField != 0)
			{
				Debug.LogWarning("Changing a ViewID while it's in use is not possible (except setting it to 0 (not being used). Current ViewID: " + viewIdField);
				return;
			}
			if (value == 0 && viewIdField != 0)
			{
				PhotonNetwork.LocalCleanPhotonView(this);
			}
			viewIdField = value;
			CreatorActorNr = value / PhotonNetwork.MAX_VIEW_IDS;
			OwnerActorNr = CreatorActorNr;
			ControllerActorNr = CreatorActorNr;
			RebuildControllerCache();
			if (value != 0)
			{
				PhotonNetwork.RegisterPhotonView(this);
			}
		}
	}

	protected internal void Awake()
	{
		if (ViewID == 0)
		{
			if (sceneViewId != 0)
			{
				ViewID = sceneViewId;
			}
			FindObservables();
		}
	}

	internal void ResetPhotonView(bool resetOwner)
	{
		lastOnSerializeDataSent = null;
	}

	internal void RebuildControllerCache(bool ownerHasChanged = false)
	{
		if (controllerActorNr == 0 || OwnerActorNr == 0 || Owner == null || Owner.IsInactive)
		{
			int num2 = (ControllerActorNr = PhotonNetwork.MasterClient?.ActorNumber ?? (-1));
			OwnerActorNr = num2;
		}
		else
		{
			ControllerActorNr = OwnerActorNr;
		}
	}

	public void OnPreNetDestroy(PhotonView rootView)
	{
		UpdateCallbackLists();
		if (OnPreNetDestroyCallbacks != null)
		{
			int i = 0;
			for (int count = OnPreNetDestroyCallbacks.Count; i < count; i++)
			{
				OnPreNetDestroyCallbacks[i].OnPreNetDestroy(rootView);
			}
		}
	}

	protected internal void OnDestroy()
	{
		if (!removedFromLocalViewList && PhotonNetwork.LocalCleanPhotonView(this) && InstantiationId > 0 && !ConnectionHandler.AppQuits && PhotonNetwork.LogLevel >= PunLogLevel.Informational)
		{
			Debug.Log("PUN-instantiated '" + base.gameObject.name + "' got destroyed by engine. This is OK when loading levels. Otherwise use: PhotonNetwork.Destroy().");
		}
	}

	[Obsolete("Use RequestableOwnershipGuard")]
	public void RequestOwnership()
	{
	}

	[Obsolete("Use RequestableOwnershipGuard")]
	public void TransferOwnership(Player newOwner)
	{
	}

	[Obsolete("Use RequestableOwnershipGuard")]
	public void TransferOwnership(int newOwnerId)
	{
	}

	public void FindObservables(bool force = false)
	{
		if (force || observableSearch != 0)
		{
			if (ObservedComponents == null)
			{
				ObservedComponents = new List<Component>();
			}
			else
			{
				ObservedComponents.Clear();
			}
			base.transform.GetNestedComponentsInChildren<Component, IPunObservable, PhotonView>(force || observableSearch == ObservableSearch.AutoFindAll, ObservedComponents);
		}
	}

	public void SerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (ObservedComponents == null || ObservedComponents.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < ObservedComponents.Count; i++)
		{
			if (ObservedComponents[i] != null)
			{
				SerializeComponent(ObservedComponents[i], stream, info);
			}
		}
	}

	public void DeserializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (ObservedComponents == null || ObservedComponents.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < ObservedComponents.Count; i++)
		{
			Component component = ObservedComponents[i];
			if (component != null)
			{
				DeserializeComponent(component, stream, info);
			}
		}
	}

	protected internal void DeserializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component is IPunObservable punObservable)
		{
			punObservable.OnPhotonSerializeView(stream, info);
		}
		else
		{
			Debug.LogError("Observed scripts have to implement IPunObservable. " + component?.ToString() + " does not. It is Type: " + component.GetType(), component.gameObject);
		}
	}

	protected internal void SerializeComponent(Component component, PhotonStream stream, PhotonMessageInfo info)
	{
		if (component is IPunObservable punObservable)
		{
			punObservable.OnPhotonSerializeView(stream, info);
		}
		else
		{
			Debug.LogError("Observed scripts have to implement IPunObservable. " + component?.ToString() + " does not. It is Type: " + component.GetType(), component.gameObject);
		}
	}

	public void RefreshRpcMonoBehaviourCache()
	{
		RpcMonoBehaviours = GetComponents<MonoBehaviour>();
	}

	public void RPC(string methodName, RpcTarget target, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, encrypt: false, parameters);
	}

	public void RpcSecure(string methodName, RpcTarget target, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, target, encrypt, parameters);
	}

	public void RPC(string methodName, Player targetPlayer, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, encrypt: false, parameters);
	}

	public void RpcSecure(string methodName, Player targetPlayer, bool encrypt, params object[] parameters)
	{
		PhotonNetwork.RPC(this, methodName, targetPlayer, encrypt, parameters);
	}

	public static PhotonView Get(Component component)
	{
		return component.transform.GetParentComponent<PhotonView>();
	}

	public static PhotonView Get(GameObject gameObj)
	{
		return gameObj.transform.GetParentComponent<PhotonView>();
	}

	public static PhotonView Find(int viewID)
	{
		return PhotonNetwork.GetPhotonView(viewID);
	}

	public void AddCallbackTarget(IPhotonViewCallback obj)
	{
		CallbackChangeQueue.Enqueue(new CallbackTargetChange(obj, null, add: true));
	}

	public void RemoveCallbackTarget(IPhotonViewCallback obj)
	{
		CallbackChangeQueue.Enqueue(new CallbackTargetChange(obj, null, add: false));
	}

	public void AddCallback<T>(IPhotonViewCallback obj) where T : class, IPhotonViewCallback
	{
		CallbackChangeQueue.Enqueue(new CallbackTargetChange(obj, typeof(T), add: true));
	}

	public void RemoveCallback<T>(IPhotonViewCallback obj) where T : class, IPhotonViewCallback
	{
		CallbackChangeQueue.Enqueue(new CallbackTargetChange(obj, typeof(T), add: false));
	}

	private void UpdateCallbackLists()
	{
		while (CallbackChangeQueue.Count > 0)
		{
			CallbackTargetChange callbackTargetChange = CallbackChangeQueue.Dequeue();
			IPhotonViewCallback obj = callbackTargetChange.obj;
			Type type = callbackTargetChange.type;
			bool add = callbackTargetChange.add;
			if (type == null)
			{
				TryRegisterCallback(obj, ref OnPreNetDestroyCallbacks, add);
				TryRegisterCallback(obj, ref OnOwnerChangeCallbacks, add);
				TryRegisterCallback(obj, ref OnControllerChangeCallbacks, add);
			}
			else if (type == typeof(IOnPhotonViewPreNetDestroy))
			{
				RegisterCallback(obj as IOnPhotonViewPreNetDestroy, ref OnPreNetDestroyCallbacks, add);
			}
			else if (type == typeof(IOnPhotonViewOwnerChange))
			{
				RegisterCallback(obj as IOnPhotonViewOwnerChange, ref OnOwnerChangeCallbacks, add);
			}
			else if (type == typeof(IOnPhotonViewControllerChange))
			{
				RegisterCallback(obj as IOnPhotonViewControllerChange, ref OnControllerChangeCallbacks, add);
			}
		}
	}

	private void TryRegisterCallback<T>(IPhotonViewCallback obj, ref List<T> list, bool add) where T : class, IPhotonViewCallback
	{
		if (obj is T obj2)
		{
			RegisterCallback(obj2, ref list, add);
		}
	}

	private void RegisterCallback<T>(T obj, ref List<T> list, bool add) where T : class, IPhotonViewCallback
	{
		if (list == null)
		{
			list = new List<T>();
		}
		if (add)
		{
			if (!list.Contains(obj))
			{
				list.Add(obj);
			}
		}
		else if (list.Contains(obj))
		{
			list.Remove(obj);
		}
	}

	public override string ToString()
	{
		return string.Format("View {0}{3} on {1} {2}", ViewID, (base.gameObject != null) ? base.gameObject.name : "GO==null", IsRoomView ? "(scene)" : string.Empty, (Prefix > 0) ? ("lvl" + Prefix) : "");
	}
}
