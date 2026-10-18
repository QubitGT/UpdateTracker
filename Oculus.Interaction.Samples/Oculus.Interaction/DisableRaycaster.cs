using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction;

public class DisableRaycaster : MonoBehaviour
{
	public float minAlpha;

	public GraphicRaycaster raycaster;

	public CanvasGroup group;

	private void Update()
	{
		raycaster.enabled = group.alpha > minAlpha;
	}
}
