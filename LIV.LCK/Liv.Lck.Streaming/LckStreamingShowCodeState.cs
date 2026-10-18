using System;
using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Core;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckStreamingShowCodeState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		controller.SetNotificationStreamCode("Loading...");
		controller.ShowNotification(NotificationType.EnterStreamCode);
		GetCodeFromCore(controller, controller.CancellationTokenSource.Token);
	}

	private async Task GetCodeFromCore(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently waiting to get code from core");
			Result<string> result = await controller.LckCore.StartLoginAttemptAsync();
			if (result.IsOk)
			{
				string notificationStreamCode = result.Ok.Insert(3, "-");
				controller.SetNotificationStreamCode(notificationStreamCode);
				WaitForUserToPairTablet(controller, controller.CancellationTokenSource.Token);
				break;
			}
			switch (result.Err)
			{
			case CoreError.InternalError:
				controller.LogError($"Internal error checking the StartLoginAttemptAsync: {result.Err} - {result.Message}");
				controller.SwitchState(controller.InternalErrorState);
				return;
			case CoreError.InvalidArgument:
				controller.LogError($"Invalid Argument error while running StartLoginAttemptAsync: {result.Err} - {result.Message}");
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
				controller.SwitchState(controller.GetCurrentState);
				return;
			case CoreError.UserNotLoggedIn:
				await Task.Delay(3000, cancellationToken);
				break;
			}
		}
	}

	private async Task WaitForUserToPairTablet(LckStreamingController controller, CancellationToken cancellationToken)
	{
		DateTime startTime = DateTime.UtcNow;
		while (!cancellationToken.IsCancellationRequested)
		{
			controller.Log("currently waiting for user to pair tablet");
			if (DateTime.UtcNow - startTime >= TimeSpan.FromMinutes(15.0))
			{
				LoginAttemptExpired(controller);
				break;
			}
			Result<bool> result = await controller.LckCore.CheckLoginCompletedAsync();
			if (result.IsOk)
			{
				if (result.Ok)
				{
					controller.SwitchState(controller.WaitingForConfigureState);
					break;
				}
			}
			else
			{
				switch (result.Err)
				{
				case CoreError.InternalError:
					controller.LogError($"Internal error while running CheckLoginCompletedAsync: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InternalErrorState);
					return;
				case CoreError.InvalidArgument:
					controller.LogError($"Invalid Argument error while running CheckLoginCompletedAsync: {result.Err} - {result.Message}");
					controller.SwitchState(controller.InvalidArgumentState);
					return;
				case CoreError.MissingTrackingId:
					controller.LogError($"MissingTrackingId error please make sure Tracking ID is setup correctly in LCK Settings: {result.Err} - {result.Message}");
					controller.SwitchState(controller.MissingTrackingIdState);
					return;
				case CoreError.LoginAttemptExpired:
					LoginAttemptExpired(controller);
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
					controller.SwitchState(controller.GetCurrentState);
					return;
				case CoreError.UserNotLoggedIn:
					break;
				}
			}
			await Task.Delay(2500, cancellationToken);
		}
	}

	private void LoginAttemptExpired(LckStreamingController controller)
	{
		controller.Log("Login request timed out after 15 mins, switching to camera mode");
		controller.ToggleCameraPage();
	}
}
