using System;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;

[AddComponentMenu("Affordance System/Rendering/Material Instance Helper", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering.MaterialInstanceHelper.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class MaterialInstanceHelper : MaterialHelperBase
{
	private Material m_MaterialInstance;

	protected void OnDestroy()
	{
		if (m_MaterialInstance != null)
		{
			Object.Destroy(m_MaterialInstance);
			m_MaterialInstance = null;
		}
	}

	public bool TryGetMaterialInstance(out Material materialInstance)
	{
		if (!base.isInitialized)
		{
			materialInstance = null;
			return false;
		}
		materialInstance = m_MaterialInstance;
		return true;
	}

	protected override void Initialize()
	{
		if (m_MaterialInstance == null)
		{
			Material[] sharedMaterials = base.rendererTarget.sharedMaterials;
			m_MaterialInstance = new Material(sharedMaterials[base.materialIndex]);
			sharedMaterials[base.materialIndex] = m_MaterialInstance;
			base.rendererTarget.sharedMaterials = sharedMaterials;
			base.Initialize();
		}
	}
}
