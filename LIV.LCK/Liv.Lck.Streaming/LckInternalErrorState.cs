using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckInternalErrorState : LckStreamingBaseState
{
	private static int _enterInternalErrorStateCount;

	public override void EnterState(LckStreamingController controller)
	{
		if (_enterInternalErrorStateCount < 5)
		{
			_enterInternalErrorStateCount++;
		}
		controller.ShowNotification(NotificationType.InternalError);
		CheckInternalError(controller, controller.CancellationTokenSource.Token);
	}

	private async Task CheckInternalError(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently checking Internal Error state");
			if (_enterInternalErrorStateCount >= 2)
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
				case CoreError.InternalError:
				case CoreError.ServiceUnavailable:
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
