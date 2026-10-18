using UnityEngine;

namespace Oculus.Interaction;

public class DeprecatedPrefab : MonoBehaviour
{
	public static readonly string label = "This prefab has been deprecated. Consider using using the replacement provided or unpack the prefab before upgrading to a new version in order to avoid losing information.";

	[SerializeField]
	[HideInInspector]
	private Object _replacement;

	[SerializeField]
	[HideInInspector]
	private bool _supressWarning;

	protected virtual void Start()
	{
		if (!_supressWarning)
		{
			string text = "#3366ff";
			string text2 = base.gameObject.name;
			Debug.LogWarning("At GameObject <color=" + text + "><b>" + text2 + "</b></color>. " + label, this);
		}
	}
}
