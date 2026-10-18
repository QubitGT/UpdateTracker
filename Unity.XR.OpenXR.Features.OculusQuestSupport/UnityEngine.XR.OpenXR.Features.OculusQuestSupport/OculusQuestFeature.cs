using System;

namespace UnityEngine.XR.OpenXR.Features.OculusQuestSupport;

[Obsolete("OpenXR.Features.OculusQuestSupport.OculusQuestFeature is deprecated. Please use OpenXR.Features.MetaQuestSupport.MetaQuestFeature instead.", false)]
public class OculusQuestFeature : OpenXRFeature
{
	public const string featureId = "com.unity.openxr.feature.oculusquest";

	public bool targetQuest = true;

	public bool targetQuest2 = true;
}
