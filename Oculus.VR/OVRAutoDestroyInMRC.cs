using UnityEngine;

public class OVRAutoDestroyInMRC : MonoBehaviour
{
	private void Start()
	{
		bool flag = false;
		Transform parent = base.transform.parent;
		while (parent != null)
		{
			if (parent.gameObject.name.StartsWith("OculusMRC_"))
			{
				flag = true;
				break;
			}
			parent = parent.parent;
		}
		if (flag)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
