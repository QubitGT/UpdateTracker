using Unity.Profiling;

namespace Drawing;

internal static class CommandBuilderSamplers
{
	internal static readonly ProfilerMarker MarkerConvert = new ProfilerMarker("Convert");

	internal static readonly ProfilerMarker MarkerSetLayout = new ProfilerMarker("SetLayout");

	internal static readonly ProfilerMarker MarkerUpdateVertices = new ProfilerMarker("UpdateVertices");

	internal static readonly ProfilerMarker MarkerUpdateIndices = new ProfilerMarker("UpdateIndices");

	internal static readonly ProfilerMarker MarkerSubmesh = new ProfilerMarker("Submesh");

	internal static readonly ProfilerMarker MarkerUpdateBuffer = new ProfilerMarker("UpdateComputeBuffer");

	internal static readonly ProfilerMarker MarkerProcessCommands = new ProfilerMarker("Commands");

	internal static readonly ProfilerMarker MarkerCreateTriangles = new ProfilerMarker("CreateTriangles");
}
