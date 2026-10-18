using UnityEngine;

namespace Oculus.Interaction.Samples;

public class EnableTargetOnStart : MonoBehaviour
{
	public MonoBehaviour[] _components;

	public GameObject[] _gameObjects;

	private void Start()
	{
		if (_components != null)
		{
			MonoBehaviour[] components = _components;
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = true;
			}
		}
		if (_gameObjects != null)
		{
			GameObject[] gameObjects = _gameObjects;
			for (int i = 0; i < gameObjects.Length; i++)
			{
				gameObjects[i].SetActive(value: true);
			}
		}
	}
}
