namespace UnityEngine.Rendering.Universal;

internal enum DecalTechniqueOption
{
	[Tooltip("Automatically selects technique based on build platform.")]
	Automatic,
	[Tooltip("Renders decals into DBuffer and then applied during opaque rendering. Requires DepthNormal prepass which makes not viable solution for the tile based renderers common on mobile.")]
	[InspectorName("DBuffer")]
	DBuffer,
	[Tooltip("Renders decals after opaque objects with normal reconstructed from depth. The decals are simply rendered as mesh on top of opaque ones, as result does not support blending per single surface data (etc. normal blending only).")]
	ScreenSpace
}
