using System;

namespace UnityEngine.Rendering;

[Serializable]
public enum ProbeVolumeBlendingTextureMemoryBudget
{
	MemoryBudgetLow = 0x80,
	MemoryBudgetMedium = 0x100,
	MemoryBudgetHigh = 0x200
}
