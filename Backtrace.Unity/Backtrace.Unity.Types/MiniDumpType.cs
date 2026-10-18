namespace Backtrace.Unity.Types;

public enum MiniDumpType : uint
{
	None = 524286u,
	Normal = 0u,
	WithDataSegs = 1u,
	WithFullMemory = 2u,
	WithHandleData = 4u,
	FilterMemory = 8u,
	ScanMemory = 16u,
	WithUnloadedModules = 32u,
	WithIndirectlyReferencedMemory = 64u,
	FilterModulePaths = 128u,
	WithProcessThreadData = 256u,
	WithPrivateReadWriteMemory = 512u,
	WithoutOptionalData = 1024u,
	WithFullMemoryInfo = 2048u,
	WithThreadInfo = 4096u,
	WithCodeSegs = 8192u,
	WithoutAuxiliaryState = 16384u,
	WithFullAuxiliaryState = 32768u,
	WithPrivateWriteCopyMemory = 65536u,
	IgnoreInaccessibleMemory = 131072u,
	ValidTypeFlags = 262143u
}
