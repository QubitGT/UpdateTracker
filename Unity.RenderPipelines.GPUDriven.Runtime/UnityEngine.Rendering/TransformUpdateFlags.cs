using System;

namespace UnityEngine.Rendering;

[Flags]
internal enum TransformUpdateFlags : byte
{
	None = 0,
	HasLightProbeCombined = 1,
	IsPartOfStaticBatch = 2
}
