using UnityEngine;

namespace Liv.Lck.Tablet;

public abstract class LckBaseNotification : MonoBehaviour
{
	[field: SerializeField]
	[field: Header("Settings")]
	public bool RemainOnScreen { get; private set; }

	[field: SerializeField]
	[field: Header("Duration To Show On Screen When Remain On Screen Is False")]
	public float ShowDuration { get; private set; } = 3f;


	public GameObject SpawnedGameObject { get; private set; }

	public virtual void ShowNotification()
	{
		if (SpawnedGameObject != null)
		{
			SpawnedGameObject.SetActive(value: true);
		}
	}

	public virtual void HideNotification()
	{
		if (SpawnedGameObject != null)
		{
			SpawnedGameObject.SetActive(value: false);
		}
	}

	public void SetSpawnedGameObject(GameObject go)
	{
		SpawnedGameObject = go;
	}
}
