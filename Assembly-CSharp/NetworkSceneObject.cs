using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkSceneObject : MonoBehaviour
{
	public PhotonView photonView;

	public bool IsMine => photonView.IsMine;

	protected virtual void Start()
	{
		if (photonView == null)
		{
			photonView = GetComponent<PhotonView>();
		}
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}
}
