using UnityEngine;

public class CanvasGroupAlphaToggle : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public float animationSpeed;

	private bool visible;

	public void ToggleVisible()
	{
		visible = !visible;
	}

	private void Start()
	{
	}

	private void Update()
	{
		canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, visible ? 1f : 0f, animationSpeed * Time.deltaTime);
	}
}
