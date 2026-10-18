namespace UnityEngine.Rendering.Universal;

internal class ShapeProviderUtility
{
	public static void CallOnBeforeRender(ShadowShape2DProvider shapeProvider, Component component, ShadowMesh2D shadowMesh, Bounds bounds)
	{
		if (component != null)
		{
			if (shapeProvider != null && component.gameObject.activeInHierarchy)
			{
				shapeProvider.OnBeforeRender(component, bounds, shadowMesh);
			}
		}
		else if (shadowMesh != null && shadowMesh.mesh != null)
		{
			shadowMesh.mesh.Clear();
		}
	}

	public static void PersistantDataCreated(ShadowShape2DProvider shapeProvider, Component component, ShadowMesh2D shadowMesh)
	{
		if (component != null)
		{
			shapeProvider?.OnPersistantDataCreated(component, shadowMesh);
		}
	}
}
