using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal interface ILight2DCullResult
{
	List<Light2D> visibleLights { get; }

	HashSet<ShadowCasterGroup2D> visibleShadows { get; }

	LightStats GetLightStatsByLayer(int layerID, ref LayerBatch layer);

	bool IsSceneLit();
}
