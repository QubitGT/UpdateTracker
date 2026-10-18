using System;

namespace UnityEngine.Rendering.RenderGraphModule;

[Flags]
internal enum RenderGraphState
{
	Idle = 0,
	RecordingGraph = 1,
	RecordingPass = 2,
	Executing = 4,
	Active = 7
}
