using System;

namespace Unity.Jobs;

[Obsolete("'IJobParallelForFilter' has been deprecated; use 'IJobFilter' instead. (UnityUpgradable) -> IJobFilter")]
public interface IJobParallelForFilter
{
	bool Execute(int index);
}
