using System.Collections.Generic;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class AISpawnManager : MonoBehaviour
{
	private Dictionary<int, GameObject> enemyTypes = new Dictionary<int, GameObject>(64);

	public static AISpawnManager? instance;

	private static bool hasInstance;

	private Dictionary<string, AISpawnPoint> spawnPoints = new Dictionary<string, AISpawnPoint>(128);

	public static bool HasInstance => hasInstance;

	private void Awake()
	{
		if (instance != null)
		{
			Object.Destroy(this);
			return;
		}
		instance = this;
		hasInstance = true;
		GetEnemyTypeTemplates();
		FindSpawnPoints();
	}

	public void FindSpawnPoints()
	{
		spawnPoints.Clear();
		AISpawnPoint[] array = Object.FindObjectsByType<AISpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			spawnPoints.Add(array[i].spawnID, array[i]);
		}
	}

	public bool GetSpawnPoint(string spawnPointID, out AISpawnPoint spawnPoint)
	{
		if (!spawnPoints.TryGetValue(spawnPointID, out spawnPoint))
		{
			return false;
		}
		return true;
	}

	public bool GetEnemyType(int enemyTypeIndex, out GameObject? newEnemy)
	{
		if (!enemyTypes.ContainsKey(enemyTypeIndex))
		{
			newEnemy = null;
			return false;
		}
		newEnemy = enemyTypes[enemyTypeIndex];
		return true;
	}

	public bool SpawnEnemy(string spawnPointID, int enemyTypeIndex, out AIAgent? newEnemy)
	{
		if (!enemyTypes.ContainsKey(enemyTypeIndex))
		{
			Debug.Log("AISpawnManager::SpawnEnemy enemy index incorrect");
			newEnemy = null;
			return false;
		}
		if (!spawnPoints.TryGetValue(spawnPointID, out AISpawnPoint value))
		{
			Debug.Log("AISpawnManager::SpawnEnemy Can't find spawn point");
			newEnemy = null;
			return false;
		}
		GameObject gameObject = Object.Instantiate(enemyTypes[enemyTypeIndex], value.transform);
		value.spawnCount++;
		newEnemy = gameObject.GetComponent<AIAgent>();
		return true;
	}

	public bool SpawnEnemy(int enemyTypeIndex, out AIAgent? newEnemy)
	{
		if (!enemyTypes.ContainsKey(enemyTypeIndex))
		{
			Debug.Log("AISpawnManager::SpawnEnemy enemy index incorrect");
			newEnemy = null;
			return false;
		}
		GameObject gameObject = Object.Instantiate(enemyTypes[enemyTypeIndex]);
		newEnemy = gameObject.GetComponent<AIAgent>();
		return true;
	}

	private void GetEnemyTypeTemplates()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			AIAgent component = child.GetComponent<AIAgent>();
			if (!(component == null) && component.isTemplate)
			{
				child.gameObject.SetActive(value: false);
				enemyTypes[component.enemyTypeId] = child.gameObject;
			}
		}
	}
}
