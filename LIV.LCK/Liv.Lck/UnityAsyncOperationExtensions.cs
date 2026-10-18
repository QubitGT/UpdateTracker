using System.Threading.Tasks;
using UnityEngine;

namespace Liv.Lck;

public static class UnityAsyncOperationExtensions
{
	public static Task AsTask(this AsyncOperation op)
	{
		TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
		op.completed += delegate
		{
			tcs.SetResult(null);
		};
		return tcs.Task;
	}
}
