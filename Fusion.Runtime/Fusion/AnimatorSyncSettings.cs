using System;

namespace Fusion;

[Serializable]
[Flags]
internal enum AnimatorSyncSettings
{
	ParameterInts = 1,
	ParameterFloats = 2,
	ParameterBools = 4,
	ParameterTriggers = 8,
	StateRoot = 0x10,
	StateLayers = 0x20,
	LayerWeights = 0x40
}
