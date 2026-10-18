using System;
using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckRateLimiterBackoffState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		controller.ShowNotification(NotificationType.RateLimiterBackoff);
		WaitForRateLimiter(controller, controller.CancellationTokenSource.Token);
	}

	private async Task WaitForRateLimiter(LckStreamingController controller, CancellationToken cancellationToken)
	{
		if (!cancellationToken.IsCancellationRequested)
		{
			Result<float> result = await controller.LckCore.GetRemainingBackoffTimeSeconds();
			int num = 10000;
			if (result.IsOk)
			{
				num = (int)Math.Truncate(result.Ok) * 1000;
				controller.Log("Got remaining backoff time in milliseconds: " + num);
			}
			else
			{
				controller.Log("Unable to get remaining backoff time, waiting 10 seconds instead");
			}
			if (num < 1000)
			{
				controller.Log("delay was: " + num + " increasing to 3 seconds to avoid looping");
				num = 3000;
			}
			await Task.Delay(num, cancellationToken);
			controller.SwitchState(controller.GetCurrentState);
		}
	}
}
