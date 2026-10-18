namespace UnityEngine.Rendering.Universal;

public abstract class ShadowShape2DProvider
{
	public virtual string ProviderName(string componentName)
	{
		return componentName;
	}

	public virtual int Priority()
	{
		return 0;
	}

	public virtual void Enabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
	}

	public virtual void Disabled(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
	}

	public abstract bool IsShapeSource(Component sourceComponent);

	public virtual void OnPersistantDataCreated(Component sourceComponent, ShadowShape2D persistantShadowShape)
	{
	}

	public virtual void OnBeforeRender(Component sourceComponent, Bounds worldCullingBounds, ShadowShape2D persistantShadowShape)
	{
	}
}
