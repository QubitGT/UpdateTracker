namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public static class LocomotionStateExtensions
{
	public static bool IsActive(this LocomotionState state)
	{
		if (state != LocomotionState.Preparing)
		{
			return state == LocomotionState.Moving;
		}
		return true;
	}
}
