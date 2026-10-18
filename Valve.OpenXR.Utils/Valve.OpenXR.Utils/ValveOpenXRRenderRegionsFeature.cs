using UnityEngine.XR.OpenXR.Features;

namespace Valve.OpenXR.Utils;

public class ValveOpenXRRenderRegionsFeature : OpenXRFeature
{
	public const string featureId = "com.valvesoftware.openxr.utils.render_regions";

	private const string extensionStrings = "";

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
