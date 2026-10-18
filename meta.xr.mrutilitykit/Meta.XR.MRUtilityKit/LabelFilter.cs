using System;
using System.Collections.Generic;

namespace Meta.XR.MRUtilityKit;

public struct LabelFilter
{
	public MRUKAnchor.SceneLabels? SceneLabels;

	public MRUKAnchor.ComponentType? ComponentTypes;

	public LabelFilter(MRUKAnchor.SceneLabels? labelFlags = null, MRUKAnchor.ComponentType? componentTypes = null)
	{
		SceneLabels = labelFlags;
		ComponentTypes = componentTypes;
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public static LabelFilter Included(List<string> included)
	{
		return Included(Utilities.StringLabelsToEnum(included));
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public static LabelFilter Excluded(List<string> excluded)
	{
		return Excluded(Utilities.StringLabelsToEnum(excluded));
	}

	[Obsolete("Use 'Included()' instead.")]
	public static LabelFilter FromEnum(MRUKAnchor.SceneLabels labels)
	{
		return Included(labels);
	}

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public bool PassesFilter(List<string> labels)
	{
		return PassesFilter(Utilities.StringLabelsToEnum(labels));
	}

	[Obsolete("Use `new LabelFilter(labelFlags)` instead")]
	public static LabelFilter Included(MRUKAnchor.SceneLabels labelFlags)
	{
		return new LabelFilter(labelFlags);
	}

	[Obsolete("Use `new LabelFilter(~labelFlags)` instead")]
	public static LabelFilter Excluded(MRUKAnchor.SceneLabels labelFlags)
	{
		LabelFilter result = default(LabelFilter);
		result.SceneLabels = ~labelFlags;
		result.ComponentTypes = null;
		return result;
	}

	public bool PassesFilter(MRUKAnchor.SceneLabels labelFlags)
	{
		if (SceneLabels.HasValue)
		{
			return (SceneLabels.Value & labelFlags) != 0;
		}
		return true;
	}
}
