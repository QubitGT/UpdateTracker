using UnityEngine;

namespace Modio.Unity.UI.Input;

public class ModioUIInputListenerLoader : MonoBehaviour
{
	[SerializeField]
	private string[] _prefabNames;

	[SerializeField]
	private string _fallbackPrefabName;

	private void Awake()
	{
		bool flag = true;
		string[] prefabNames = _prefabNames;
		for (int i = 0; i < prefabNames.Length; i++)
		{
			GameObject gameObject = Resources.Load<GameObject>(prefabNames[i]);
			if (!(gameObject == null))
			{
				Object.Instantiate(gameObject, base.transform);
				flag = false;
			}
		}
		if (flag && !string.IsNullOrEmpty(_fallbackPrefabName))
		{
			GameObject gameObject2 = Resources.Load<GameObject>(_fallbackPrefabName);
			if (gameObject2 != null)
			{
				Object.Instantiate(gameObject2, base.transform);
			}
		}
	}
}
