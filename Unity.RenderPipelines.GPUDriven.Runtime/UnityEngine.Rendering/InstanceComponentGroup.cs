using System;

namespace UnityEngine.Rendering;

[Flags]
internal enum InstanceComponentGroup : uint
{
	Default = 1u,
	Wind = 2u,
	LightProbe = 4u,
	Lightmap = 8u,
	DefaultWind = 3u,
	DefaultLightProbe = 5u,
	DefaultLightmap = 9u,
	DefaultWindLightProbe = 7u,
	DefaultWindLightmap = 0xBu
}
