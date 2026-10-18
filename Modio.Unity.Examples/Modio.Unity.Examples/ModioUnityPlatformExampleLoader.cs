using System;
using System.Linq;
using UnityEngine;

namespace Modio.Unity.Examples;

public class ModioUnityPlatformExampleLoader : MonoBehaviour
{
	[Serializable]
	private class PlatformExamples
	{
		public RuntimePlatform[] platforms;

		public string[] prefabNames;
	}

	[SerializeField]
	private PlatformExamples[] platformExamplesPerPlatform;

	private void Awake()
	{
		RuntimePlatform platform = Application.platform;
		PlatformExamples[] array = platformExamplesPerPlatform;
		foreach (PlatformExamples platformExamples in array)
		{
			if (!platformExamples.platforms.Contains(platform))
			{
				continue;
			}
			string[] prefabNames = platformExamples.prefabNames;
			foreach (string text in prefabNames)
			{
				GameObject gameObject = Resources.Load<GameObject>(text);
				if (gameObject != null)
				{
					Debug.Log($"Instantiating platform {text} for platform {platform}");
					UnityEngine.Object.Instantiate(gameObject, base.transform);
				}
				else
				{
					Debug.LogError($"Couldn't find expected platformExample {text} for platform {platform}");
				}
			}
		}
	}

	[ContextMenu("TestAllPrefabNamesAreFound")]
	private void TestAllPrefabNamesAreFound()
	{
		bool flag = false;
		PlatformExamples[] array = platformExamplesPerPlatform;
		foreach (PlatformExamples platformExamples in array)
		{
			string[] prefabNames = platformExamples.prefabNames;
			foreach (string text in prefabNames)
			{
				if (Resources.Load<GameObject>(text) == null)
				{
					Debug.LogError($"Couldn't find expected platformExample {text} for platform {platformExamples.platforms.FirstOrDefault()}");
					flag = true;
				}
			}
		}
		if (!flag)
		{
			Debug.Log("No issues found");
		}
	}
}
