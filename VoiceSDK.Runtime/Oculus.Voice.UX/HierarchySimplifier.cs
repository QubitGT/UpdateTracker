using UnityEngine;

namespace Oculus.Voice.UX;

public class HierarchySimplifier : MonoBehaviour
{
	[Tooltip("Whether to hide the object on startup, by default.")]
	[SerializeField]
	public bool hideByDefault = true;

	public static void HideSubObjects(GameObject obj, bool hideObjects)
	{
		HierarchySimplifier[] componentsInChildren = obj.GetComponentsInChildren<HierarchySimplifier>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			ToggleShowInHierarchyFlag(componentsInChildren[i].gameObject, hideObjects);
		}
	}

	private void OnValidate()
	{
		ToggleShowInHierarchyFlag(base.gameObject, hideByDefault);
	}

	public static void ToggleShowInHierarchyFlag(GameObject obj, bool hideObject)
	{
		obj.hideFlags = (hideObject ? (obj.hideFlags | HideFlags.HideInHierarchy) : (obj.hideFlags & ~HideFlags.HideInHierarchy));
	}
}
