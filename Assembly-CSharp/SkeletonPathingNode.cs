using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Skeleton Pathing Node")]
public class SkeletonPathingNode : MonoBehaviour
{
	public bool ejectionPoint;

	public SkeletonPathingNode[] connectedNodes;

	public float distanceToExitNode;

	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}
}
