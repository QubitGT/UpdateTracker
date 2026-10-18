using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction;

[RequireComponent(typeof(RectTransform))]
[ExecuteInEditMode]
public class RectTransformBoundsClipperDriver : MonoBehaviour
{
	[SerializeField]
	private BoundsClipper _boundsClipper;

	protected virtual void Awake()
	{
		Resize();
	}

	protected virtual void Start()
	{
	}

	private void OnRectTransformDimensionsChange()
	{
		Resize();
	}

	private void Resize()
	{
		if (!(_boundsClipper == null))
		{
			RectTransform rectTransform = base.transform as RectTransform;
			_boundsClipper.Size = new Vector3(rectTransform.rect.width, rectTransform.rect.height, 0.01f);
		}
	}
}
