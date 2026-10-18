using System;
using UnityEngine.UI;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects;

[Serializable]
[DisplayName("UI Graphic", null)]
[CustomTrackedObject(typeof(Graphic), true)]
public class TrackedUGuiGraphic : JsonSerializerTrackedObject
{
	protected override void PostApplyTrackedProperties()
	{
		((Graphic)base.Target).SetAllDirty();
		base.PostApplyTrackedProperties();
	}
}
