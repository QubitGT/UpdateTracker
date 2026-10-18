using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckServiceUnavailableState : LckStreamingBaseState
{
	private static int _enterServiceUnavailableStateCount;

	public override void EnterState(LckStreamingController controller)
	{
		if (_enterServiceUnavailableStateCount < 5)
		{
			_enterServiceUnavailableStateCount++;
		}
		controller.ShowNotification(NotificationType.ServiceUnavailable);
		CheckServiceStatus(controller, controller.CancellationTokenSource.Token);
	}

	private async Task CheckServiceStatus(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently checking Service Unavailable state");
			if (_enterServiceUnavailableStateCount >= 2)
			{
				await Task.Delay(10000, cancellationToken);
			}
			Result<bool> result = await controller.LckCore.HasUserConfiguredStreaming();
			if (!result.IsOk)
			{
				switch (result.Err)
				{
				case CoreError.UserNotLoggedIn:
					controller.SwitchState(controller.ShowCodeState);
					return;
				case CoreError.InternalError:
					controller.LogError($"Internal error checking backend: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InternalErrorState);
					break;
				case CoreError.InvalidArgument:
					controller.LogError($"Invalid Argument error checking HasUserConfiguredStreaming: {result.Err} - {result.Message}");
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
				default:
					controller.LogError("Tried to check an LCKCore Error missing from this switch statement");
					await Task.Delay(5000, cancellationToken);
					controller.SwitchState(controller.GetCurrentState);
					return;
				case CoreError.ServiceUnavailable:
					break;
				}
				await Task.Delay(7000, cancellationToken);
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
