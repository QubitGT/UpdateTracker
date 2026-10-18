using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public enum OVRRenderingMode
{
	[InspectorName("Alpha-Blended")]
	AlphaBlended = 0,
	[InspectorName("Alpha-Cutout")]
	AlphaCutout = 1,
	[InspectorName("Opaque")]
	Opaque = 2,
	[InspectorName("OVR/Overlay")]
	Overlay = 100,
	[InspectorName("OVR/Underlay")]
	Underlay = 101
}
