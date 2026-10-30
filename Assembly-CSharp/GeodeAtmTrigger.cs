using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("GorillaTag/Cosmetics/Geode Atm Trigger")]
public class GeodeAtmTrigger : MonoBehaviour
{
	[SerializeField]
	private UnityEvent OnTrigger;

	private void OnTriggerEnter(Collider other)
	{
		if (OnTrigger != null)
		{
			OnTrigger.Invoke();
		}
	}
}
