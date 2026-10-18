using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public enum RenderingMode
{
	[InspectorName("Alpha-Blended")]
	AlphaBlended,
	[InspectorName("Alpha-Cutout")]
	AlphaCutout,
	[InspectorName("Opaque")]
	Opaque
}
