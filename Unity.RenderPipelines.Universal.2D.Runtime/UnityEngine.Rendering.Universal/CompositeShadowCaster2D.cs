using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[AddComponentMenu("Rendering/2D/Composite Shadow Caster 2D")]
[MovedFrom(false, "UnityEngine.Experimental.Rendering.Universal", "com.unity.render-pipelines.universal", null)]
[ExecuteInEditMode]
public class CompositeShadowCaster2D : ShadowCasterGroup2D
{
	protected void OnEnable()
	{
		ShadowCasterGroup2DManager.AddGroup(this);
	}

	protected void OnDisable()
	{
		ShadowCasterGroup2DManager.RemoveGroup(this);
	}
}
