using System;
using Unity.Jobs;

namespace Unity.Collections;

public interface INativeDisposable : IDisposable
{
	JobHandle Dispose(JobHandle inputDeps);
}
