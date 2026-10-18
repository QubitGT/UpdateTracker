using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

internal static class OVRFuture
{
	public static async OVRTask<OVRPlugin.Result> When(ulong future, CancellationToken cancellationToken = default(CancellationToken))
	{
		CheckCancellationAndThrow(future, cancellationToken);
		OVRPlugin.Result result;
		OVRPlugin.FutureState state;
		while ((result = LogIfNotSuccess(OVRPlugin.PollFuture(future, out state), "Unable to poll for future state: {0}")).IsSuccess() && state == OVRPlugin.FutureState.Pending)
		{
			await Task.Yield();
			CheckCancellationAndThrow(future, cancellationToken);
		}
		return result;
		static void CheckCancellationAndThrow(ulong futureToCancel, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				LogIfNotSuccess(OVRPlugin.CancelFuture(futureToCancel), "Unable to cancel future: {0}");
				throw new OperationCanceledException("Future was canceled.", token);
			}
		}
		static OVRPlugin.Result LogIfNotSuccess(OVRPlugin.Result value, string msg)
		{
			if (!value.IsSuccess())
			{
				Debug.LogError(string.Format(msg, value));
			}
			return value;
		}
	}
}
