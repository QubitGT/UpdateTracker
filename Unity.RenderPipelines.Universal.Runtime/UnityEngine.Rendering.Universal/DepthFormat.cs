namespace UnityEngine.Rendering.Universal;

public enum DepthFormat
{
	[RenderPathCompatible(RenderPathCompatibility.All)]
	Default = 0,
	[RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
	Depth_16 = 90,
	[RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
	Depth_24 = 91,
	[RenderPathCompatible(RenderPathCompatibility.Forward | RenderPathCompatibility.ForwardPlus)]
	Depth_32 = 93,
	[RenderPathCompatible(RenderPathCompatibility.All)]
	Depth_16_Stencil_8 = 151,
	[RenderPathCompatible(RenderPathCompatibility.All)]
	Depth_24_Stencil_8 = 92,
	[RenderPathCompatible(RenderPathCompatibility.All)]
	Depth_32_Stencil_8 = 94
}
