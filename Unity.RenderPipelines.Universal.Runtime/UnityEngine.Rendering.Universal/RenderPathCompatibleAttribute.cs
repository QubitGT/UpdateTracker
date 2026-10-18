using System;

namespace UnityEngine.Rendering.Universal;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class RenderPathCompatibleAttribute : Attribute
{
	public RenderPathCompatibility renderPath;

	public RenderPathCompatibleAttribute(RenderPathCompatibility renderPath)
	{
		this.renderPath = renderPath;
	}
}
