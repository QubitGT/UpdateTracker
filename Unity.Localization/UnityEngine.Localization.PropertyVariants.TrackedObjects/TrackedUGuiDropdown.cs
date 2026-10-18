using System;
using UnityEngine.UI;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects;

[Serializable]
[DisplayName("UI Dropdown", null)]
[CustomTrackedObject(typeof(Dropdown), true)]
public class TrackedUGuiDropdown : JsonSerializerTrackedObject
{
	protected override void PostApplyTrackedProperties()
	{
		((Dropdown)base.Target).RefreshShownValue();
		base.PostApplyTrackedProperties();
	}
}
