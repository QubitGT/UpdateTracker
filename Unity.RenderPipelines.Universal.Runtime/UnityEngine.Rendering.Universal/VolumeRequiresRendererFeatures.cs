using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class VolumeRequiresRendererFeatures : Attribute
{
	internal HashSet<Type> TargetFeatureTypes;

	public VolumeRequiresRendererFeatures(params Type[] featureTypes)
	{
		TargetFeatureTypes = ((featureTypes != null) ? new HashSet<Type>(featureTypes) : new HashSet<Type>());
		TargetFeatureTypes.Remove(null);
	}
}
