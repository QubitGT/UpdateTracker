using UnityEngine;

namespace Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Photon Networking/Photon Rigidbody View")]
public class PhotonRigidbodyView : MonoBehaviourPun, IPunObservable
{
	private float m_Distance;

	private float m_Angle;

	private Rigidbody m_Body;

	private Vector3 m_NetworkPosition;

	private Quaternion m_NetworkRotation;

	[HideInInspector]
	public bool m_SynchronizeVelocity = true;

	[HideInInspector]
	public bool m_SynchronizeAngularVelocity;

	[HideInInspector]
	public bool m_TeleportEnabled;

	[HideInInspector]
	public float m_TeleportIfDistanceGreaterThan = 3f;

	public void Awake()
	{
		m_Body = GetComponent<Rigidbody>();
		m_NetworkPosition = default(Vector3);
		m_NetworkRotation = default(Quaternion);
	}

	public void FixedUpdate()
	{
		if (!base.photonView.IsMine)
		{
			m_Body.position = Vector3.MoveTowards(m_Body.position, m_NetworkPosition, m_Distance * (1f / (float)PhotonNetwork.SerializationRate));
			m_Body.rotation = Quaternion.RotateTowards(m_Body.rotation, m_NetworkRotation, m_Angle * (1f / (float)PhotonNetwork.SerializationRate));
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(m_Body.position);
			stream.SendNext(m_Body.rotation);
			if (m_SynchronizeVelocity)
			{
				stream.SendNext(m_Body.linearVelocity);
			}
			if (m_SynchronizeAngularVelocity)
			{
				stream.SendNext(m_Body.angularVelocity);
			}
			return;
		}
		m_NetworkPosition = (Vector3)stream.ReceiveNext();
		m_NetworkRotation = (Quaternion)stream.ReceiveNext();
		if (m_TeleportEnabled && Vector3.Distance(m_Body.position, m_NetworkPosition) > m_TeleportIfDistanceGreaterThan)
		{
			m_Body.position = m_NetworkPosition;
		}
		if (m_SynchronizeVelocity || m_SynchronizeAngularVelocity)
		{
			float num = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
			if (m_SynchronizeVelocity)
			{
				m_Body.linearVelocity = (Vector3)stream.ReceiveNext();
				m_NetworkPosition += m_Body.linearVelocity * num;
				m_Distance = Vector3.Distance(m_Body.position, m_NetworkPosition);
			}
			if (m_SynchronizeAngularVelocity)
			{
				m_Body.angularVelocity = (Vector3)stream.ReceiveNext();
				m_NetworkRotation = Quaternion.Euler(m_Body.angularVelocity * num) * m_NetworkRotation;
				m_Angle = Quaternion.Angle(m_Body.rotation, m_NetworkRotation);
			}
		}
	}
}
