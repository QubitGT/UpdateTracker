using UnityEngine;

namespace Photon.Pun;

public class MonoBehaviourPun : MonoBehaviour
{
	private PhotonView pvCache;

	public PhotonView photonView
	{
		get
		{
			if (pvCache == null)
			{
				pvCache = PhotonView.Get(this);
			}
			return pvCache;
		}
	}
}
