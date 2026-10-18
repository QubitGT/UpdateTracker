using System;
using DigitalOpus.MB.Core;
using UnityEngine;

public class MB3_BatchPrefabBaker : MonoBehaviour
{
	[Serializable]
	public class MB3_PrefabBakerRow
	{
		public GameObject sourcePrefab;

		public GameObject resultPrefab;
	}

	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	[NonReorderable]
	public MB3_PrefabBakerRow[] prefabRows = new MB3_PrefabBakerRow[0];

	public string outputPrefabFolder = "";

	[ContextMenu("Create Instances For Prefab Rows")]
	public void CreateSourceAndResultPrefabInstances()
	{
		Debug.LogError("Cannot be used outside the editor");
	}
}
