using System;

namespace UnityEngine.Rendering;

[Serializable]
[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\PostProcessing\\LensFlareDataSRP.cs")]
public enum SRPLensFlareType
{
	Image,
	Circle,
	Polygon,
	Ring,
	LensFlareDataSRP
}
