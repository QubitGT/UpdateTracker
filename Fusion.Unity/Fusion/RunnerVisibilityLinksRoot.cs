using UnityEngine;

namespace Fusion;

[AddComponentMenu("")]
internal class RunnerVisibilityLinksRoot : MonoBehaviour
{
	private void Awake()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}
}
