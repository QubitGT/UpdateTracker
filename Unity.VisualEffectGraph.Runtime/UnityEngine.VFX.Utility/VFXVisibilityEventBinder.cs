namespace UnityEngine.VFX.Utility;

[RequireComponent(typeof(Renderer))]
internal class VFXVisibilityEventBinder : VFXEventBinderBase
{
	public enum Activation
	{
		OnBecameVisible,
		OnBecameInvisible
	}

	public Activation activation;

	protected override void SetEventAttribute(object[] parameters)
	{
	}

	private void OnBecameVisible()
	{
		if (activation == Activation.OnBecameVisible)
		{
			SendEventToVisualEffect();
		}
	}

	private void OnBecameInvisible()
	{
		if (activation == Activation.OnBecameInvisible)
		{
			SendEventToVisualEffect();
		}
	}
}
