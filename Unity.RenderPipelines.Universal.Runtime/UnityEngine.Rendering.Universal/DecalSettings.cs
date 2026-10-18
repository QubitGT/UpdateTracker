using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
internal class DecalSettings
{
	public DecalTechniqueOption technique;

	public float maxDrawDistance = 1000f;

	public bool decalLayers;

	public DBufferSettings dBufferSettings;

	public DecalScreenSpaceSettings screenSpaceSettings;
}
