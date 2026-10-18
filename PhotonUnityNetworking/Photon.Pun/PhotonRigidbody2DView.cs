using UnityEngine;

namespace Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("Photon Networking/Photon Rigidbody 2D View")]
public class PhotonRigidbody2DView : MonoBehaviourPun, IPunObservable
{
	private float m_Distance;

	private float m_Angle;

	private Rigidbody2D m_Body;

	private Vector2 m_NetworkPosition;

	private float m_NetworkRotation;

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
		m_Body = GetComponent<Rigidbody2D>();
		m_NetworkPosition = default(Vector2);
	}

	public void FixedUpdate()
	{
		if (!base.photonView.IsMine)
		{
			m_Body.position = Vector2.MoveTowards(m_Body.position, m_NetworkPosition, m_Distance * (1f / (float)PhotonNetwork.SerializationRate));
			m_Body.rotation = Mathf.MoveTowards(m_Body.rotation, m_NetworkRotation, m_Angle * (1f / (float)PhotonNetwork.SerializationRate));
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
		m_NetworkPosition = (Vector2)stream.ReceiveNext();
		m_NetworkRotation = (float)stream.ReceiveNext();
		if (m_TeleportEnabled && Vector3.Distance(m_Body.position, m_NetworkPosition) > m_TeleportIfDistanceGreaterThan)
		{
			m_Body.position = m_NetworkPosition;
		}
		if (m_SynchronizeVelocity || m_SynchronizeAngularVelocity)
		{
			float num = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
			if (m_SynchronizeVelocity)
			{
				m_Body.linearVelocity = (Vector2)stream.ReceiveNext();
				m_NetworkPosition += m_Body.linearVelocity * num;
				m_Distance = Vector2.Distance(m_Body.position, m_NetworkPosition);
			}
			if (m_SynchronizeAngularVelocity)
			{
				m_Body.angularVelocity = (float)stream.ReceiveNext();
				m_NetworkRotation += m_Body.angularVelocity * num;
				m_Angle = Mathf.Abs(m_Body.rotation - m_NetworkRotation);
			}
		}
	}
}
