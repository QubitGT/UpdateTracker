using System;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace Valve.OpenXR.Utils;

public class ValveOpenXRLeptonValidationFeature : OpenXRFeature
{
	public const string featureId = "com.valvesoftware.openxr.utils.validation";

	private Type[] incompatibleFeatureTypes = new Type[1] { typeof(MetaQuestFeature) };
}
