using UnityEngine;

namespace Fusion.Statistics;

[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Statistics/Statistics World Anchor")]
public class FusionStatsWorldAnchor : MonoBehaviour
{
	private void OnEnable()
	{
		FusionStatsConfig.SetWorldAnchorCandidate(base.transform, register: true);
	}

	private void OnDisable()
	{
		FusionStatsConfig.SetWorldAnchorCandidate(base.transform, register: false);
	}

	private void OnDestroy()
	{
		FusionStatsCanvas componentInChildren = base.transform.GetComponentInChildren<FusionStatsCanvas>();
		if ((bool)componentInChildren)
		{
			componentInChildren.transform.SetParent(null);
			componentInChildren.GetComponentInChildren<FusionStatsConfig>(includeInactive: true).ResetToCanvasAnchor();
		}
	}
}
