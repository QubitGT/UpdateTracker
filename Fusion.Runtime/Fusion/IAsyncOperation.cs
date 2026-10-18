using System;
using System.Runtime.ExceptionServices;

namespace Fusion;

public interface IAsyncOperation
{
	bool IsDone { get; }

	ExceptionDispatchInfo Error { get; }

	event Action<IAsyncOperation> Completed;
}
