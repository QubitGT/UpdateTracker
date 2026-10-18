using UnityEngine;

namespace Liv.Lck.DependencyInjection;

[DefaultExecutionOrder(-800)]
public class LckDependencyResolver : MonoBehaviour
{
	private void Awake()
	{
		LckMonoBehaviourDependencyInjector injector = LckDiContainer.Instance.GetInjector();
		if (injector == null)
		{
			Debug.LogError("LCK initialization error: Ensure LckServiceInitializer is in the scene");
			return;
		}
		MonoBehaviour[] components = base.gameObject.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour instance in components)
		{
			injector.Inject(instance);
		}
	}
}
