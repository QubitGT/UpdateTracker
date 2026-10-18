using System.Collections;
using System.Threading;
using UnityEngine;

public class MainThreadUtil : MonoBehaviour
{
	public static MainThreadUtil Instance { get; private set; }

	public static SynchronizationContext synchronizationContext { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void Setup()
	{
		Instance = new GameObject("MainThreadUtil").AddComponent<MainThreadUtil>();
		synchronizationContext = SynchronizationContext.Current;
	}

	public static void Run(IEnumerator waitForUpdate)
	{
		synchronizationContext.Post(delegate
		{
			Instance.StartCoroutine(waitForUpdate);
		}, null);
	}

	private void Awake()
	{
		base.gameObject.hideFlags = HideFlags.HideAndDontSave;
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
