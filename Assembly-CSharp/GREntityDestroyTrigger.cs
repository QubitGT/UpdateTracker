using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Entity Destroy Trigger")]
public class GREntityDestroyTrigger : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		GameEntity component = other.attachedRigidbody.GetComponent<GameEntity>();
		if (component != null && component.IsAuthority())
		{
			component.manager.RequestDestroyItem(component.id);
		}
	}
}
