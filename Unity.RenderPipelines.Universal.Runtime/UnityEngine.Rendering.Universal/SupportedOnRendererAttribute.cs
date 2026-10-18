using System;

namespace UnityEngine.Rendering.Universal;

[AttributeUsage(AttributeTargets.Class)]
public class SupportedOnRendererAttribute : Attribute
{
	public Type[] rendererTypes { get; }

	public SupportedOnRendererAttribute(Type renderer)
		: this(new Type[1] { renderer })
	{
	}

	public SupportedOnRendererAttribute(params Type[] renderers)
	{
		if (renderers == null)
		{
			Debug.LogError("The SupportedOnRendererAttribute parameters cannot be null.");
			return;
		}
		foreach (Type type in renderers)
		{
			if (type == null || !typeof(ScriptableRendererData).IsAssignableFrom(type))
			{
				Debug.LogError("The SupportedOnRendererAttribute Attribute targets an invalid ScriptableRendererData. One of the types cannot be assigned from ScriptableRendererData");
				return;
			}
		}
		rendererTypes = renderers;
	}
}
