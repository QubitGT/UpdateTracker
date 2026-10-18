using System;

namespace UnityEngine.Rendering;

[Serializable]
public enum ProbeVolumeTextureMemoryBudget
{
	MemoryBudgetLow = 0x200,
	MemoryBudgetMedium = 0x400,
	MemoryBudgetHigh = 0x800
}
