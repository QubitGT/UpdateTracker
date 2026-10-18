using System;

namespace UnityEngine.ProBuilder;

[Flags]
public enum CullingMode
{
	None = 0,
	Back = 1,
	Front = 2,
	FrontBack = 3
}
