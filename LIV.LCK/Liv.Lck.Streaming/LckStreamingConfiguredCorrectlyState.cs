namespace Liv.Lck.Streaming;

public class LckStreamingConfiguredCorrectlyState : LckStreamingBaseState
{
	public override void EnterState(LckStreamingController controller)
	{
		controller.HideNotifications();
	}
}
