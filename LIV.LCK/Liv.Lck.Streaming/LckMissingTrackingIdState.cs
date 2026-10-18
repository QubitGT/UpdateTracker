using System.Threading;
using System.Threading.Tasks;
using Liv.Lck.Tablet;

namespace Liv.Lck.Streaming;

public class LckMissingTrackingIdState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		controller.ShowNotification(NotificationType.MissingTrackingId);
		SwitchStateAfterDelay(controller, controller.CancellationTokenSource.Token);
	}

	private async Task SwitchStateAfterDelay(LckStreamingController controller, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			await Task.Delay(7000, cancellationToken);
			controller.SwitchState(controller.GetCurrentState);
		}
	}
}
