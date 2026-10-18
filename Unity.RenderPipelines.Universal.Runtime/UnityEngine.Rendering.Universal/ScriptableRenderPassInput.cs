using System;

namespace UnityEngine.Rendering.Universal;

[Flags]
public enum ScriptableRenderPassInput
{
	None = 0,
	Depth = 1,
	Normal = 2,
	Color = 4,
	Motion = 8
}
