using System;

namespace Unity.Burst.Intrinsics;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[BurstRuntime.Preserve]
internal sealed class BurstTargetCpuAttribute : Attribute
{
	public readonly BurstTargetCpu TargetCpu;

	public BurstTargetCpuAttribute(BurstTargetCpu TargetCpu)
	{
		this.TargetCpu = TargetCpu;
	}
}
