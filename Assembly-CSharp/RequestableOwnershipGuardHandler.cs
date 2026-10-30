using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;

internal class RequestableOwnershipGuardHandler : IPunOwnershipCallbacks, IInRoomCallbacks
{
	private static HashSet<NetworkView> gaurdedViews;

	private static readonly RequestableOwnershipGuardHandler callbackInstance;

	private static Dictionary<NetworkView, RequestableOwnershipGuard> guardingLookup;

	static RequestableOwnershipGuardHandler()
	{
		gaurdedViews = new HashSet<NetworkView>();
		callbackInstance = new RequestableOwnershipGuardHandler();
		guardingLookup = new Dictionary<NetworkView, RequestableOwnershipGuard>();
		PhotonNetwork.AddCallbackTarget(callbackInstance);
	}

	internal static void RegisterView(NetworkView view, RequestableOwnershipGuard guard)
	{
		if (!(view == null) && !gaurdedViews.Contains(view))
		{
			gaurdedViews.Add(view);
			guardingLookup.Add(view, guard);
		}
	}

	internal static void RemoveView(NetworkView view)
	{
		if (!(view == null))
		{
			gaurdedViews.Remove(view);
			guardingLookup.Remove(view);
		}
	}

	internal static void RegisterViews(NetworkView[] views, RequestableOwnershipGuard guard)
	{
		for (int i = 0; i < views.Length; i++)
		{
			RegisterView(views[i], guard);
		}
	}

	public static void RemoveViews(NetworkView[] views, RequestableOwnershipGuard guard)
	{
		for (int i = 0; i < views.Length; i++)
		{
			RemoveView(views[i]);
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
	{
		NetworkView networkView = gaurdedViews.FirstOrDefault((NetworkView p) => p.GetView == targetView);
		if (!networkView.IsNull() && guardingLookup.TryGetValue(networkView, out var value) && !value.IsNull())
		{
			Player player = value.currentOwner?.GetPlayerRef();
			int num = player?.ActorNumber ?? 0;
			if (num == 0 || previousOwner != player)
			{
				GTDev.LogWarning("Ownership transferred but the previous owner didn't initiate the request, Switching back");
				targetView.OwnerActorNr = num;
				targetView.ControllerActorNr = num;
			}
		}
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		OnHostChangedShared();
	}

	private void OnHostChangedShared()
	{
		foreach (NetworkView gaurdedView in gaurdedViews)
		{
			if (!guardingLookup.TryGetValue(gaurdedView, out var value))
			{
				break;
			}
			if (gaurdedView.Owner != null && value.currentOwner != null && !object.Equals(gaurdedView.Owner, value.currentOwner))
			{
				gaurdedView.OwnerActorNr = value.currentOwner.ActorNumber;
				gaurdedView.ControllerActorNr = value.currentOwner.ActorNumber;
			}
		}
	}

	void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
	{
	}

	void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
	{
	}

	public void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}
}
