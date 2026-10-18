using System;
using UnityEngine.UI;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects;

[Serializable]
[DisplayName("Layout Group", null)]
[CustomTrackedObject(typeof(LayoutGroup), true)]
public class TrackedLayoutGroup : JsonSerializerTrackedObject
{
	protected override void PostApplyTrackedProperties()
	{
		if (base.Target is LayoutGroup { transform: RectTransform transform })
		{
			LayoutRebuilder.MarkLayoutForRebuild(transform);
		}
		base.PostApplyTrackedProperties();
	}
}
