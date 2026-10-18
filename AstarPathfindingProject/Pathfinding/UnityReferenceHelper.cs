using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[ExecuteInEditMode]
[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_unity_reference_helper.php")]
public class UnityReferenceHelper : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private string guid;

	public string GetGUID()
	{
		return guid;
	}

	public void Awake()
	{
		Reset();
	}

	public void Reset()
	{
		if (string.IsNullOrEmpty(guid))
		{
			guid = Guid.NewGuid().ToString();
			Debug.Log("Created new GUID - " + guid, this);
		}
		else
		{
			if (base.gameObject.scene.name == null)
			{
				return;
			}
			UnityReferenceHelper[] array = Object.FindObjectsByType<UnityReferenceHelper>(FindObjectsSortMode.None);
			foreach (UnityReferenceHelper unityReferenceHelper in array)
			{
				if (unityReferenceHelper != this && guid == unityReferenceHelper.guid)
				{
					guid = Guid.NewGuid().ToString();
					Debug.Log("Created new GUID - " + guid, this);
					break;
				}
			}
		}
	}
}
