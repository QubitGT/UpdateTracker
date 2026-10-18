using System;

namespace Unity.Burst;

[Flags]
internal enum NativeDumpFlags
{
	None = 0,
	IL = 1,
	Unused = 2,
	IR = 4,
	IROptimized = 8,
	Asm = 0x10,
	Function = 0x20,
	Analysis = 0x40,
	IRPassAnalysis = 0x80,
	ILPre = 0x100,
	IRPerEntryPoint = 0x200,
	All = 0x3FD
}
