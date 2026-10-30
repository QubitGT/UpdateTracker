using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Rigid Body Noise Event Maker")]
public class GRRigidBodyNoiseEventMaker : MonoBehaviour
{
	public float velocityThreshold = 5f;

	public void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > velocityThreshold && GetComponent<GameEntity>() != null)
		{
			GRNoiseEventManager.instance.AddNoiseEvent(collision.GetContact(0).point);
		}
	}
}
