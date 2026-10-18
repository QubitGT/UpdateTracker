using UnityEngine;

namespace Pathfinding.Examples;

[ExecuteInEditMode]
[HelpURL("http://arongranberg.com/astar/documentation/stable/class_snap_to_node.php")]
public class SnapToNode : MonoBehaviour
{
	private void Update()
	{
		if (base.transform.hasChanged && AstarPath.active != null)
		{
			GraphNode node = AstarPath.active.GetNearest(base.transform.position, NNConstraint.None).node;
			if (node != null)
			{
				base.transform.position = (Vector3)node.position;
				base.transform.hasChanged = false;
			}
		}
	}
}
