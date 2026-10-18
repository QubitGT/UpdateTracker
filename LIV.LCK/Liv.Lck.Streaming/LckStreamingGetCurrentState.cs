using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;

namespace Liv.Lck.Streaming;

public class LckStreamingGetCurrentState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		GetCurrentState(controller, controller.CancellationTokenSource.Token);
	}

	private async Task GetCurrentState(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently waiting for get current state");
			Result<bool> result = await controller.LckCore.HasUserConfiguredStreaming();
			if (!result.IsOk)
			{
				switch (result.Err)
				{
				case CoreError.UserNotLoggedIn:
					controller.SwitchState(controller.ShowCodeState);
					return;
				case CoreError.InternalError:
					controller.LogError($"Internal error checking if user is Configured: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InternalErrorState);
					return;
				case CoreError.InvalidArgument:
					controller.LogError($"Invalid Argument error checking if user is Configured: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InvalidArgumentState);
					return;
				case CoreError.MissingTrackingId:
					controller.LogError($"MissingTrackingId error please make sure Tracking ID is setup correctly in LCK Settings: {result.Err} - {result.Message}");
					controller.SwitchState(controller.MissingTrackingIdState);
					return;
				case CoreError.RateLimiterBackoff:
					controller.LogError($"Too many requests sent to our backend error: {result.Err} - {result.Message}");
					controller.SwitchState(controller.RateLimiterBackoffState);
					return;
				case CoreError.ServiceUnavailable:
					controller.LogError($"Unable to reach our backend error: {result.Err} - {result.Message}");
					controller.SwitchState(controller.ServiceUnavailableState);
					return;
				default:
					controller.LogError("Tried to check an LCKCore Error missing from this switch statement");
					await Task.Delay(5000, cancellationToken);
					break;
				}
				continue;
			}
			if (result.Ok)
			{
				controller.SwitchState(controller.ConfiguredCorrectlyState);
			}
			else
			{
				controller.SwitchState(controller.WaitingForConfigureState);
			}
			break;
		}
	}
}
