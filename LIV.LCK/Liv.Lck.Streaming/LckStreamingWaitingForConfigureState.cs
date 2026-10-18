using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckStreamingWaitingForConfigureState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		controller.ShowNotification(NotificationType.ConfigureStream);
		CheckConfiguredState(controller, controller.CancellationTokenSource.Token);
	}

	private async Task CheckConfiguredState(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently waiting for configured state");
			Result<bool> result = await controller.LckCore.HasUserConfiguredStreaming();
			if (!result.IsOk)
			{
				switch (result.Err)
				{
				case CoreError.UserNotLoggedIn:
					controller.SwitchState(controller.ShowCodeState);
					break;
				case CoreError.InternalError:
					controller.LogError($"Internal error checking if user is Configured: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InternalErrorState);
					break;
				case CoreError.InvalidArgument:
					controller.LogError($"Invalid Argument error checking if user is Configured: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InvalidArgumentState);
					break;
				case CoreError.MissingTrackingId:
					controller.LogError($"MissingTrackingId error please make sure Tracking ID is setup correctly in LCK Settings: {result.Err} - {result.Message}");
					controller.SwitchState(controller.MissingTrackingIdState);
					break;
				case CoreError.RateLimiterBackoff:
					controller.LogError($"Too many requests sent to our backend error: {result.Err} - {result.Message}");
					controller.SwitchState(controller.RateLimiterBackoffState);
					break;
				case CoreError.ServiceUnavailable:
					controller.LogError($"Unable to reach our backend error: {result.Err} - {result.Message}");
					controller.SwitchState(controller.ServiceUnavailableState);
					break;
				default:
					controller.LogError("Tried to check an LCKCore Error missing from this switch statement");
					await Task.Delay(5000, cancellationToken);
					controller.SwitchState(controller.GetCurrentState);
					break;
				}
				break;
			}
			if (result.Ok)
			{
				controller.SwitchState(controller.ConfiguredCorrectlyState);
				break;
			}
			await Task.Delay(2500, cancellationToken);
		}
	}
}
