using System;

namespace Meta.XR.Util;

[AttributeUsage(AttributeTargets.Class)]
internal class FeatureAttribute : Attribute
{
	public Feature Feature { get; }

	public FeatureAttribute(Feature feature)
	{
		Feature = feature;
	}
}
