using UnityEngine;

namespace Fusion;

public class NetworkObjectPrefabData : Behaviour
{
	public NetworkObjectGuid Guid;

	private void OnValidate()
	{
		if (Application.isEditor && base.gameObject.scene.IsValid())
		{
			base.hideFlags |= HideFlags.HideAndDontSave | HideFlags.HideInInspector;
		}
	}
}
