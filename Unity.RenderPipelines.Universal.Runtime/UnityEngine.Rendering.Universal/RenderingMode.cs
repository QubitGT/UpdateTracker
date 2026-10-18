namespace UnityEngine.Rendering.Universal;

public enum RenderingMode
{
	Forward = 0,
	[InspectorName("Forward+")]
	ForwardPlus = 2,
	Deferred = 1,
	[InspectorName("Deferred+")]
	DeferredPlus = 3
}
